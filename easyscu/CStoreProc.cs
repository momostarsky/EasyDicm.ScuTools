using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dicom;
using Dicom.IO.Reader;
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
        protected ConcurrentBag<string> UIFormatFailed;
        protected ConcurrentBag<string> DicomValFailed;

        public CStoreProc(StoreOptions opt) : base(opt)
        {
            SopItems = new ConcurrentBag<KeyValuePair<string, string>>();
            UIFormatFailed = new ConcurrentBag<String>();
            DicomValFailed = new ConcurrentBag<string>();
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
                DicomFile df = null;
                try
                {
                    df = await DicomFile.OpenAsync(dicomFiles[i]);
                }
                catch (Exception e)
                {
                    Log.Error(dicomFiles[i] + ":不是合法的DICOM文件");
                }

                if (df == null)
                {
                    continue;
                }

                if (!df.Dataset.Contains(DicomTag.SOPClassUID))
                {
                    Log.Error(dicomFiles[i] + ":不是合法的DICOM文件  Tag.SOPClassUID 不存在  ");
                    continue;
                }

                if (!df.Dataset.Contains(DicomTag.SOPInstanceUID))
                {
                    Log.Error(":不是合法的DICOM文件  Tag.SOPInstanceUID 不存在  ");
                    continue;
                }

                DicomCStoreRequest request = null;
                Exception error = null;

                try
                {
                    request = new DicomCStoreRequest(df);
                    request.OnResponseReceived += (req, response) =>
                        Console.WriteLine("C-Store Response Received, Status: " + response.Status);
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                if (request == null)
                {
                    DicomValFailed.Add(dicomFiles[i]);
                    continue;
                }

                var sopclsuid = df.Dataset.GetString(DicomTag.SOPClassUID);
                var sopuid = df.Dataset.GetString(DicomTag.SOPInstanceUID);
                var ok = false;
                try
                {
                    DicomValidation.ValidateUI(sopuid);
                    ok = true;
                }
                catch (DicomValidationException e)
                {
                    UIFormatFailed.Add(dicomFiles[i]);
                }

                if (ok)
                {
                    SopItems.Add(new KeyValuePair<string, string>(sopuid, sopclsuid));
                    await client.AddRequestAsync(request);
                }
                else
                {
                    request = null;
                }
            }

            if (SopItems.Count > 0)
            {
                await client.SendAsync();
            }
            else
            {
                client = null;
                Console.WriteLine("No Dicom Files To Send!");
            }
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

        private async Task SendStorageCommit()
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
                DicomDataset ds = new DicomDataset()
                {
                    {DicomTag.ReferencedSOPClassUID, sopInfo.Value},
                    {DicomTag.ReferencedSOPInstanceUID, sopInfo.Key}
                };


                dicomRefSopSequence.Items.Add(ds);
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

            if (SopItems.Count > 0)
            {
                await SendStorageCommit( );
            }


            Random rd = new Random(DateTime.Now.Millisecond);
            foreach (var kv in UIFormatFailed)
            {
                FileInfo f = new FileInfo(kv);

                File.Move(kv, $"./uidfmt-{f.Name}{rd.Next()}.dcm");
            }

            foreach (var kv in DicomValFailed)
            {
                FileInfo f = new FileInfo(kv);

                File.Move(kv, $"./uidmis-{f.Name}{rd.Next()}.dcm");
            }
        }
    }
}