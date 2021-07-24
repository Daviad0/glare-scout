using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;

namespace LightScout
{
    public class StorageManager
    {
        private static readonly object l1 = new object();
        public static StorageManager instance = null;
        private static IFolder rootFolder = FileSystem.Current.LocalStorage;
        public static StorageManager Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new StorageManager();
                    }
                    return instance;
                }

            }
        }

        public async Task<string> GetData(string fileName)
        {
            IFolder folder = await rootFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
            if(folder != null)
            {
                // folder exists, yay
                IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                if(file != null)
                {
                    return await file.ReadAllTextAsync();
                }
            }
            return null;
            
        }

        public async Task<string> SetData(string fileName, string data)
        {
            IFolder folder = await rootFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            await file.WriteAllTextAsync(data);
            return await file.ReadAllTextAsync();
        }
    }
}
