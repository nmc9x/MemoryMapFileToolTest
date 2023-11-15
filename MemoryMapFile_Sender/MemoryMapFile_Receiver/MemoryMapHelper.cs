using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryMapFile_Sender
{
    public class MemoryMapHelper : IDisposable
    {
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly Mutex _mutex;
        private readonly EventWaitHandle _dataWrittenEvent;
        private readonly EventWaitHandle _dataReadEvent;
        public bool _isDisposed = false;

        public MemoryMapHelper(string mapName, long capacity)
        {
            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(mapName, capacity);
            _accessor = _memoryMappedFile.CreateViewAccessor(0, capacity);

            // create Mutex for sync read/write
            _mutex = new Mutex(false, "MMFMutex");

            // create Event Read/Write
            _dataWrittenEvent = new EventWaitHandle(false, EventResetMode.AutoReset, mapName + "WriteEvent");
            _dataReadEvent = new EventWaitHandle(false, EventResetMode.AutoReset, mapName + "ReadEvent");
        }

        public void WriteData(byte[] data, long offset)
        {
            _mutex.WaitOne();
            try
            {
                _accessor.WriteArray(offset, data, 0, data.Length);
                _dataWrittenEvent.Set(); // Set Write Event
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            _dataReadEvent.WaitOne(); // Wait for Read
        }

        public void ReadData(long offset, int length, out byte[] res)
        {
            _ = _dataWrittenEvent.WaitOne(); // Wait For Write
            _mutex.WaitOne();
            try
            {
                res = new byte[length];
                _accessor.ReadArray(offset, res, 0, length);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            _dataReadEvent.Set(); //Set Read Event
        }


        // Code Async
        public async Task WriteDataAsync(byte[] data, long offset, int resetLength)
        {
            await Task.Run(() =>
            {
                _mutex.WaitOne();
                try
                {
                    _accessor.WriteArray(offset, new byte[resetLength], 0, resetLength);
                    _accessor.WriteArray(offset, data, 0, data.Length);
                    _dataWrittenEvent.Set();
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            });

            await Task.Run(() => _dataReadEvent.WaitOne());
        }
        public async Task<byte[]> ReadDataAsync(long offset, int length)
        {
            byte[] res = null;
            await Task.Run(() => _dataWrittenEvent.WaitOne());
            await Task.Run(() =>
            {
                _mutex.WaitOne();
                try
                {
                    res = new byte[length];
                    _accessor.ReadArray(offset, res, 0, length);
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            });

            _dataReadEvent.Set();
            return res;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _accessor.Dispose();
                    _memoryMappedFile.Dispose();
                    _dataWrittenEvent.Dispose();
                    _dataReadEvent.Dispose();
                    _mutex.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
