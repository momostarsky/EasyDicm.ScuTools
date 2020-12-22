using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Dicom;
using Dicom.Network;
using DicomClient = Dicom.Network.Client.DicomClient;


namespace easyscu
{
    /// <summary>
    /// Usage
    /// cstore --ae  DicmQRSCP   --port 11112   --host 192.168.1.92   --myae  EasySCU   --batch 3  --src ./dcmdata
    /// </summary>
    public class CStoreProc : ScuProc<StoreOptions>
    {
        protected ConcurrentBag<KeyValuePair<string, string>> SopItems;

        public CStoreProc(StoreOptions opt) : base(opt)
        {
            SopItems = new ConcurrentBag<KeyValuePair<string, string>>();
        }


        private async Task SendSubSize(string[] dicomFiles)
        {
            var client = new DicomClient(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            client.NegotiateAsyncOps();
            var exts = DicmAppInfo.Instance.DicomExtendeds.Value;
            client.AdditionalExtendedNegotiations.AddRange(exts);


            int end = dicomFiles.Length;
            for (int i = 0; i < end; i++)
            {
                Log.Info(dicomFiles[i]);
                var df = await DicomFile.OpenAsync(dicomFiles[i]);
                var request = new DicomCStoreRequest(df);
                request.OnResponseReceived += (req, response) =>
                    Console.WriteLine("C-Store Response Received, Status: " + response.Status);
                var sopclsuid = df.Dataset.GetString(DicomTag.SOPClassUID);
                var sopuid = df.Dataset.GetString(DicomTag.SOPInstanceUID);
                SopItems.Add(new KeyValuePair<string, string>(sopuid, sopclsuid));
                await client.AddRequestAsync(request);
            }

            await client.SendAsync();
        }

       static Task<DicomNEventReportResponse> OnNEventReportRequest(DicomNEventReportRequest request)
        {
            var refSopSequence = request.Dataset.GetSequence(DicomTag.ReferencedSOPSequence);
            foreach (var item in refSopSequence.Items)
            {
                Console.WriteLine("SOP Class UID: {0}", item.GetString(DicomTag.ReferencedSOPClassUID));
                Console.WriteLine("SOP Instance UID: {0}", item.GetString(DicomTag.ReferencedSOPInstanceUID));
            }

            return Task.FromResult(new DicomNEventReportResponse(request, DicomStatus.Success));
        }

        private async Task SendStorageCommit(string[] dicomFiles)
        {
            var client = new DicomClient(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            client.NegotiateAsyncOps();
            var exts = DicmAppInfo.Instance.DicomExtendeds.Value;
            client.AdditionalExtendedNegotiations.AddRange(exts);


            var txnUid = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
            var nActionDicomDataSet = new DicomDataset
            {
                {DicomTag.TransactionUID, txnUid}
            };
            var dicomRefSopSequence = new DicomSequence(DicomTag.ReferencedSOPSequence);
            // var seqItem = new DicomDataset()
            // {
            //     {DicomTag.ReferencedSOPClassUID, "1.2.840.10008.5.1.4.1.1.1"},
            //     {DicomTag.ReferencedSOPInstanceUID, "1.3.46.670589.30.2273540226.4.54"}
            // }; 
            foreach (var sopInfo in SopItems)
            {
                dicomRefSopSequence.Items.Add(new DicomDataset()
                {
                    {DicomTag.ReferencedSOPClassUID, sopInfo.Value},
                    {DicomTag.ReferencedSOPInstanceUID, sopInfo.Key}
                });
            }

            // dicomRefSopSequence.Items.Add(seqItem);


            nActionDicomDataSet.Add(dicomRefSopSequence);
            var nActionRequest = new DicomNActionRequest(DicomUID.StorageCommitmentPushModelSOPClass,
                DicomUID.StorageCommitmentPushModelSOPInstance, 1)
            {
                Dataset = nActionDicomDataSet,
                OnResponseReceived = (DicomNActionRequest request, DicomNActionResponse response) =>
                {
                    Console.WriteLine("NActionResponseHandler, response status:{0}", response.Status);
                },
            };
            await client.AddRequestAsync(nActionRequest);
            client.OnNEventReportRequest = OnNEventReportRequest;
            await client.SendAsync();
        }

        public override async Task Start()
        {
            String[] ie = System.IO.Directory.GetFiles(Opt.DicomSrc, "*", SearchOption.AllDirectories);


            int mg = ie.Length / Opt.BatchSize;
            int lf = ie.Length % Opt.BatchSize;

            if (lf > 0)
            {
                mg += 1;
            }

            var grups = new List<string[]>(mg);
            for (int i = 0; i < mg; i++)
            {
                String[] arr = null;
                if (lf > 0 && i == mg - 1)
                {
                    arr = new string[lf];
                }
                else
                {
                    arr = new string[Opt.BatchSize];
                }

                Array.Copy(ie, i * Opt.BatchSize, arr, 0, arr.Length);
                grups.Add(arr);
            }

            foreach (var grup in grups)
            {
                await SendSubSize(grup);
            }

            await SendStorageCommit(ie);
        }
    }
}