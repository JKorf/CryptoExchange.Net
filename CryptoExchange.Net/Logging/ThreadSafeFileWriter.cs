using System.IO;
using System.Text;

namespace CryptoExchange.Net.Logging
{
    public class ThreadSafeFileWriter: TextWriter
    {
        private StreamWriter logWriter;
        private object writeLock;

        public override Encoding Encoding => Encoding.ASCII;

        public ThreadSafeFileWriter(string path)
        {
            writeLock = new object();
            logWriter = new StreamWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
            logWriter.AutoFlush = true;
        }


        public override void WriteLine(string logMessage)
        {
            lock (writeLock)
                logWriter.WriteLine(logMessage);
        }

        protected override void Dispose(bool disposing)
        {
            logWriter.Close();
            logWriter = null;
        }
    }
}
