using System.Threading;
using System.Threading.Tasks;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using NUnit.Framework;
 
namespace ScuTests
{
    public class SslTest
    {
        
        [SetUp]
        public void StartUp()
        {
        
           
        }

        [TearDown]
        public void Clearup()
        {
            
        }

        [Test]
        public async Task EchoWithSSL()
        {

            int cout = 0;

            ManualResetEventSlim ev = new ManualResetEventSlim(false);
                
            var dicomReq = new DicomCEchoRequest
            {
                OnResponseReceived = (request, response) =>
                {
                    cout += 1; 
                    ev.Set();
                }
            };
         
            
            // var client = new DicomClient("192.168.1.115",5678,true,"JPAIBox","ConquestSeve1"); 
            // await client.AddRequestAsync(dicomReq); 
            // await client.SendAsync();
            // ev.Wait(1000);
            // Assert.True(cout == 1);





        }
    }
}