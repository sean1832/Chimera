using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkey.src.IO
{
    public class FileTypeInfo
    {
        public string FullName { get; set; }
        public string Extension { get; set; }
    }

    internal class FileTypeManager
    {
        private Dictionary<string, List<FileTypeInfo>> fileCategories;

        public FileTypeManager()
        {
            fileCategories = new Dictionary<string, List<FileTypeInfo>>();

            fileCategories.Add("text", new List<FileTypeInfo>
            {
                new FileTypeInfo { FullName = "Text File", Extension = "\".txt\"" },
                new FileTypeInfo { FullName = "CSV File", Extension = "\".csv\"" },
                new FileTypeInfo { FullName = "JSON File", Extension = "\".json\"" },
                new FileTypeInfo { FullName = "YAML File", Extension = "\".yml\""}
            });

            fileCategories.Add("object", new List<FileTypeInfo>
            {
                new FileTypeInfo { FullName = "Autodesk FBX (.fbx)", Extension = @""".fbx""" },
                new FileTypeInfo { FullName = "Wavefront OBJ (.obj)", Extension = @""".obj""" },
                new FileTypeInfo { FullName = "Rhino 3D Model (.3dm)", Extension = @""".3dm""" },
                new FileTypeInfo { FullName = "Stereolithography (.stl)", Extension = @""".stl""" },
                new FileTypeInfo { FullName = "3D Studio (.3ds)", Extension = @""".3ds""" },
                new FileTypeInfo { FullName = "Collada (.dae)", Extension = @""".dae""" }
            });
        }

        public Dictionary<string, List<FileTypeInfo>> GetAllCategories()
        {
            return fileCategories;
        }

        public List<FileTypeInfo> GetFileTypesByCategory(string category)
        {
            if (category == "all")
            {
                List<FileTypeInfo> allFileTypes = new List<FileTypeInfo>();
                foreach (var fileTypeList in fileCategories.Values)
                {
                    allFileTypes.AddRange(fileTypeList);
                }
                return allFileTypes;
            }
            else if (fileCategories.TryGetValue(category, out var fileTypeInfos))
            {
                return fileTypeInfos;
            }
            else
            {
                return null;
            }
        }
    }
}