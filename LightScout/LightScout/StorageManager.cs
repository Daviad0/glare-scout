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
    public class ApplicationDataHandler
    {
        public string TestSchemaString = @"{
  'id': '76628abc',
  'prettyName': 'Infinite Recharge',
  'categories': [
      {
        'prettyName' : 'Autonomous',
        'autoStart?' : true,
        'expectedStart' : null,
        'type': 'category',
        'uniqueId' : 'autonomous',
        'contents' : [
          {
            'type' : 'parent',
            'prettyName' : 'Power Cells',
            'uniqueId' : 'powerCellA_parent',
            'conditions' : 
              {
                'max' : 15,
                'min' : 0
              },
            
            'contents' : [
{
                'type' : 'text',
                'prettyName' : 'These fields can only go up to a maximum of 15 power cells added up!',
                'uniqueId' : 'powerCellA_label'
                
              },
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellA_inner',
                'conditions':{
                    'min':0,
                    'groupLock' : 'powerCellA_parent'
}                   
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Outer',
                'uniqueId' : 'powerCellA_outer',
                'conditions':{
'min':0,
                    'groupLock' : 'powerCellA_parent'
}     
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Lower',
                'uniqueId' : 'powerCellA_lower',
                'conditions':{
'min':0,
                    'groupLock' : 'powerCellA_parent'
}     
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Missed',
                'uniqueId' : 'powerCellA_missed',
                'conditions':{
'min':0,
                    'groupLock' : 'powerCellA_parent'
}     
              }
            ]
          },
            {
            'type' : 'parent',
            'prettyName' : 'Robot Tasks',
            'uniqueId' : 'initLine_parent',
            'contents' : [
              {
                'type' : 'choices',
                'prettyName' : 'Initiation Line?',
                'uniqueId' : 'initLine',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              },
            ]
          }    
        ]
      },
      {
        'prettyName' : 'Tele-Op',
        'autoStart?' : false,
        'expectedStart' : 16,
        'type': 'category',
        'uniqueId' : 'teleop',
        'contents' : [
          {
            'type' : 'parent',
            'prettyName' : 'Power Cells',
            'uniqueId' : 'powerCellT_parent',
            'conditions' : 
              {
                'max' : 5,
                'min' : 0
              },
            
            'contents' : [
{
                'type' : 'text',
                'prettyName' : 'These fields can only go up to a maximum of 5 power cells added up!',
                'uniqueId' : 'powerCellT_label'
                
              },
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellT_inner',
                'conditions':{
                    'max':5,
                    'min':0,
                    'groupLock' : 'powerCellT_parent'
}                   
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Outer',
                'uniqueId' : 'powerCellT_outer',
                'conditions':{
                    'max':5,
                    'min':0,
                    'groupLock' : 'powerCellT_parent'
}                   
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Lower',
                'uniqueId' : 'powerCellT_lower',
                'conditions':{
                    'max':5,
                    'min':0,
                    'groupLock' : 'powerCellT_parent'
}                   
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Missed',
                'uniqueId' : 'powerCellT_missed',
                'conditions':{
                    'max':5,
                    'min':0,
                    'groupLock' : 'powerCellT_parent'
}                   
              }
            ]
          },
{
            'type' : 'parent',
            'prettyName' : 'Control Panel',
            'uniqueId' : 'controlPanel_parent',
            'contents' : [
              {
                'type' : 'choices',
                'prettyName' : 'Rotation?',
                'uniqueId' : 'rotation',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              },{
                'type' : 'choices',
                'prettyName' : 'Position?',
                'uniqueId' : 'position',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              }
            ]
          }
        ]
      },
{
        'prettyName' : 'Endgame',
        'autoStart?' : false,
        'expectedStart' : 135,
        'type': 'category',
        'uniqueId' : 'endgame',
        'contents' : [
          {
            'type' : 'toggle',
            'prettyName' : 'Parked?',
            'uniqueId' : 'parked',
            'conditions' : 
                {
                    'options' : [
                        'Not Parked', 'Parked'
                    ]
                }
            
            },
{
            'type' : 'dropdown',
            'prettyName' : 'Climbed?',
            'uniqueId' : 'climbed',
            'conditions' : 
                {
                    'options' : [
                        'No','Attempted', 'Succeeded'
                    ]
                }
            
            },
{
'type' : 'timer',
            'prettyName' : 'Went for Endgame',
            'uniqueId' : 'endgame_tried'
              }
        ]
      }
    ]
}
";
        public static StorageManager StorageManager = StorageManager.Instance;
        private static readonly object l1 = new object();
        public static ApplicationDataHandler instance = null;
        public static ApplicationDataHandler Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new ApplicationDataHandler();
                    }
                    return instance;
                }

            }
        }
        public static ApplicationData CurrentApplicationData;
        public static List<Scouter> Users;
        public static List<DataEntry> AvailableEntries;
        public static List<Competition> Competitions;
        public static List<Schema> Schemas;
        public async Task InitializeData()
        {
            var existingData = await StorageManager.GetData("app_data");
            if(existingData == "" || existingData == null)
            {
                CurrentApplicationData = new ApplicationData();
                // handle maybe start screen of tablet!
            }
            else
            {
                try
                {
                    CurrentApplicationData = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationData>(existingData);
                }
                catch(Exception e)
                {
                    CurrentApplicationData = new ApplicationData();
                }
            }

            existingData = await StorageManager.GetData("matches");
            if(existingData == "" || existingData == null)
            {
                AvailableEntries = new List<DataEntry>();
                AvailableEntries.Add(new DataEntry()
                {
                    TeamIdentifier = "0001",
                    TeamName = "REEVES, David",
                    Audited = false,
                    Completed = false,
                    Competition = "72721DT",
                    Schema = "444899fa",
                    Number = 1,
                    Position = "Main",
                    AssistedBy = new List<string>() { "0002", "0003" }
                });
                AvailableEntries.Add(new DataEntry()
                {
                    TeamIdentifier = "0001",
                    TeamName = "REEVES, David",
                    Audited = false,
                    Completed = true,
                    Competition = "72721DT",
                    Schema = "444899fa",
                    Number = 2,
                    Position = "Red 1",
                    AssistedBy = new List<string>() { "0005", "0002" }
                });
                AvailableEntries.Add(new DataEntry()
                {
                    TeamIdentifier = "0001",
                    TeamName = "REEVES, David",
                    Audited = true,
                    Completed = false,
                    Competition = "72721DT",
                    Schema = "444899fa",
                    Number = 3,
                    Position = "Blue 2",
                    AssistedBy = new List<string>() { "0007", "0003" }
                });
                AvailableEntries.Add(new DataEntry()
                {
                    TeamIdentifier = "0001",
                    TeamName = "REEVES, David",
                    Audited = true,
                    Completed = true,
                    Competition = "72721DT",
                    Schema = "444899fa",
                    Number = 4,
                    Position = "Blue 2",
                    AssistedBy = new List<string>() { "0007", "0003" }
                });
            }
            else
            {
                try
                {
                    AvailableEntries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataEntry>>(existingData);
                }catch(Exception e)
                {
                    AvailableEntries = new List<DataEntry>();
                }
            }

            existingData = await StorageManager.GetData("users");
            if (existingData == "" || existingData == null)
            {
                Users = new List<Scouter>();
            }
            else
            {
                try
                {
                    Users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Scouter>>(existingData);
                }
                catch (Exception e)
                {
                    Users = new List<Scouter>();
                }
            }

            existingData = await StorageManager.GetData("competitions");
            if (existingData == "" || existingData == null)
            {
                Competitions = new List<Competition>();
                Competitions.Add(new Competition()
                {
                    Name = "FRC862 Driver Training",
                    Id = "72721DT",
                    StartsAt = DateTime.Now,
                    AllowedSchemas = new List<string>(),
                    Location = "Canton, MI"
                });
            }
            else
            {
                try
                {
                    Competitions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Competition>>(existingData);
                }
                catch (Exception e)
                {
                    Competitions = new List<Competition>();
                }
            }

            existingData = await StorageManager.GetData("schemas");
            if (existingData == "" || existingData == null)
            {
                Schemas = new List<Schema>();
                Schemas.Add(new Schema()
                {
                    Id = "444899fa",
                    Name = "2021 Driver Training A",
                    GotAt = DateTime.Now,
                    JSONData = TestSchemaString

                });
            }
            else
            {
                try
                {
                    Schemas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schema>>(existingData);
                }
                catch (Exception e)
                {
                    Schemas = new List<Schema>();
                }
            }
        }
        public async Task SaveAppData()
        {
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(CurrentApplicationData);
            await StorageManager.SetData("app_data", dataToPut);
        }
        public async Task SaveUsers()
        {
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(Users);
            await StorageManager.SetData("users", dataToPut);
        }
        public async Task SaveSchemas()
        {
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(Schemas);
            await StorageManager.SetData("schemas", dataToPut);
        }
        public async Task SaveCompetitions()
        {
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(Competitions);
            await StorageManager.SetData("competitions", dataToPut);
        }
        public async Task SaveMatches()
        {
            // need to update into the already existing ones
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(AvailableEntries);
            await StorageManager.SetData("matches", dataToPut);
        }
    }
    public class ApplicationData
    {
        public string DeviceId;
        public Announcement CurrentAnnouncement;
        public string AdminCode;
        public string CurrentCompetition;
        public string EncryptionKey;
    }
    public class Announcement
    {
        public string Title;
        public string Data;
        public DateTime GotAt;
        public DateTime ActiveUntil;
    }
    public class Scouter
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Score { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Banned { get; set; }
    }
    public class Competition
    {
        public string Name;
        public string Location;
        public string Id;
        public List<string> AllowedSchemas;
        public DateTime StartsAt;
    }
    public class Schema
    {
        public string Name;
        public string Id;
        public string JSONData;
        public DateTime GotAt;
    }
    public class DataEntry
    {
        public string Id;
        public string Competition;
        public string Schema;
        public int Number;
        public string Position;
        public string JSONData;
        public DateTime LastEdited;
        public string TeamIdentifier;
        public string TeamName;
        public bool Completed;
        public bool Audited;
        public List<string> AssistedBy;
    }
}
