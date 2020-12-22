using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace easyrsa

{
    /// <summary>
    ///  https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// https://docs.microsoft.com/zh-cn/dotnet/standard/garbage-collection/implementing-dispose
    /// </summary>
    public class BaseDispose :  IDisposable
    {
        
        // To detect redundant calls
        private bool _disposed = false;

        ~BaseDispose() => Dispose(false);
        
     
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected  virtual void ClearManagedObjects()
        {
            
           
        }
        protected  virtual void ClearUnManagedObjects()
        {
            
        }
      
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                ClearManagedObjects();
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            ClearUnManagedObjects(); 
            _disposed = true;
        }
    }
}