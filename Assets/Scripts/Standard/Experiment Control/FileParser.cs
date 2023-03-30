namespace ExperimentControl
{
    using System.IO;
    using System.Threading.Tasks;

    public class FileParser 
    {
        public static async Task<int> GetIdAsync(string file)
        {
            int id = await ReadIdAsync(file);
            return id;
        }
        
        private static async Task<int> ReadIdAsync(string file)
        {
            using StreamReader streamReader = new (file);
            char[]             buffer       = new char[(int)streamReader.BaseStream.Length];
            await streamReader.ReadAsync(buffer, 0, (int)streamReader.BaseStream.Length);
            string line = new (buffer);
            string[] values = line.Split(',');
            int value = int.Parse(values[0]);
            return value;
        }
    }
}
