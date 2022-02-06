using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LightScout.Models;
using Newtonsoft.Json;
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
            if (folder != null)
            {
                // folder exists, yay
                IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                if (file != null)
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
        public async Task<bool> DeleteData(string fileName)
        {
            try
            {
                IFolder folder = await rootFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
                IFile file = await folder.GetFileAsync(fileName);
                await file.DeleteAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }
    public class ApplicationDataHandler
    {
        public string TestSchemaString = @"{
   'categories':[
      {
         'prettyName':'Autonomous',
         'autoStart?':true,
         'expectedStart':null,
         'type':'category',
         'uniqueId':'autonomous',
         'contents':[
           {
              'type':'parent',
              'prettyName':'Alliance Human Player',
              'uniqueId':'allianceHumanPlayer_parent',
              'contents':[
                 {
                    'type':'text',
                    'prettyName':'The Alliance Human Player is on the same side as the Alliance Station.',
                    'uniqueId':'allianceHumanPlayer_Label'
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Successful Shots',
                    'uniqueId':'allianceHumanPlayer_success',
                    'conditions':{
                       'min':0,
                       'max':3
                    }
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Failed Shots',
                    'uniqueId':'allianceHumanPlayer_fail',
                    'conditions':{
                       'min':0,
                       'max':3
                    }
                 }
              ]
           },
            {
               'type':'parent',
               'prettyName':'Cargo Balls Low',
               'uniqueId':'cargoBallsALow_parent',
               'contents':[
                  {
                     'type':'text',
                     'prettyName':'Please select the number of successes and fails of the Cargo Balls LOWER',
                     'uniqueId':'cargoBallsALow_Label'
                  },
                  {
                     'type':'stepper',
                     'prettyName':'Successful',
                     'uniqueId':'cargoBallsALow_success',
                     'conditions':{
                        'min':0
                     }
                  },
                  {
                     'type':'stepper',
                     'prettyName':'Failure',
                     'uniqueId':'cargoBallsALow_fail',
                     'conditions':{
                        'min':0
                     }
                  }
               ]
            },
            {
              'type':'parent',
              'prettyName':'Cargo Balls High',
              'uniqueId':'cargoBallsAHigh_parent',
              'contents':[
                 {
                    'type':'text',
                    'prettyName':'Please select the number of successes and fails of the Cargo Balls HIGH',
                    'uniqueId':'cargoBallsAHigh_Label'
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Successful',
                    'uniqueId':'cargoBallsAHigh_success',
                    'conditions':{
                       'min':0
                    }
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Failure',
                    'uniqueId':'cargoBallsAHigh_fail',
                    'conditions':{
                       'min':0
                    }
                 }
              ]
           },
            {
               'type':'parent',
               'prettyName':'Robot Tasks',
               'uniqueId':'taxiLine_parent',
               'contents':[
                  {
                     'type':'choices',
                     'prettyName':'Fully Outside Tarmac?',
                     'uniqueId':'taxiLine',
                     'conditions':{
                        'options':[
                           'Yes',
                           'No'
                        ]
                     }
                  }
               ]
            }
         ]
      },
      {
         'prettyName':'Tele-Op',
         'autoStart?':false,
         'expectedStart':16,
         'type':'category',
         'uniqueId':'teleop',
         'contents':[
           {
              'type':'parent',
              'prettyName':'Cargo Balls Low',
              'uniqueId':'cargoBallsTLow_parent',
              'contents':[
                 {
                    'type':'text',
                    'prettyName':'Please select the number of successes and fails of the Cargo Balls LOWER',
                    'uniqueId':'cargoBallsTLow_Label'
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Successful',
                    'uniqueId':'cargoBallsTLow_success',
                    'conditions':{
                       'min':0
                    }
                 },
                 {
                    'type':'stepper',
                    'prettyName':'Failure',
                    'uniqueId':'cargoBallsTLow_fail',
                    'conditions':{
                       'min':0
                    }
                 }
              ]
           },
           {
             'type':'parent',
             'prettyName':'Cargo Balls High',
             'uniqueId':'cargoBallsTHigh_parent',
             'contents':[
                {
                   'type':'text',
                   'prettyName':'Please select the number of successes and fails of the Cargo Balls HIGH',
                   'uniqueId':'cargoBallsTHigh_Label'
                },
                {
                   'type':'stepper',
                   'prettyName':'Successful',
                   'uniqueId':'cargoBallsTHigh_success',
                   'conditions':{
                      'min':0
                   }
                },
                {
                   'type':'stepper',
                   'prettyName':'Failure',
                   'uniqueId':'cargoBallsTHigh_fail',
                   'conditions':{
                      'min':0
                   }
                }
             ]
          },
            {
               'type':'parent',
               'prettyName':'Defense',
               'uniqueId':'defense_parent',
               'contents':[
                  {
                     'type':'toggle',
                     'prettyName':'Blocking Defense?',
                     'uniqueId':'blocking',
                     'conditions':{
                        'options':[
                           'Didn't Block',
                           'Blocked'
                        ]
                     }
                  },
                  {
                     'type':'toggle',
                     'prettyName':'Hoarding Defense?',
                     'uniqueId':'hoarding',
                     'conditions':{
                        'options':[
                           'Didn't Hoard',
                           'Hoarded'
                        ]
                     }
                  }
               ]
            }
         ]
      },
      {
         'prettyName':'Hangar',
         'autoStart?':false,
         'expectedStart':135,
         'type':'category',
         'uniqueId':'hangar',
         'contents':[
           {
              'type':'text',
              'prettyName':'Click the `Went for Hangar` button once a robot tries for the objective, hold to clear!',
              'uniqueId':'hangar_label'
           },
           {
              'type':'timer',
              'prettyName':'Went for Hangar',
              'uniqueId':'hangar_tried'
           },
            {
               'type':'toggle',
               'prettyName':'Completed Rung',
               'uniqueId':'rungSuccessful',
               'conditions':{
                  'options':[
                     'Not Successful',
                     'Successful'
                  ]
               }
            },
            {
               'type':'dropdown',
               'prettyName':'Rung Level',
               'uniqueId':'rungLevel',
               'conditions':{
                  'options':[
                     'N/A',
                     'Low',
                     'Mid',
                     'High',
                     'Traversal'
                  ]
               }
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
        public static DiagnosticReport LatestDiagnosticReport;
        public static List<Scouter> Users;
        public static List<DataEntry> AvailableEntries;
        public static List<DataEntry> AllEntries;
        public static List<Competition> Competitions;
        public static List<Schema> Schemas;
        public static List<Log> Logs;
        public async Task CompileDiagnostics()
        {
            var newestDiagnosticData = new DiagnosticReport();
            newestDiagnosticData.BatteryLevel = (float)Xamarin.Essentials.Battery.ChargeLevel * 100;
        }
        public async Task InitializeData()
        {
            var existingData = await StorageManager.GetData("app_data");
            if (existingData == "" || existingData == null)
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
                catch (Exception e)
                {
                    CurrentApplicationData = new ApplicationData();
                }
            }
            //await StorageManager.DeleteData("matches");
            existingData = await StorageManager.GetData("matches");

            if (existingData == "" || existingData == null)
            {
                AllEntries = new List<DataEntry>();
                AllEntries.Add(new DataEntry()
                {
                    Id = "4a4a4a01",
                    TeamIdentifier = "0001",
                    TeamName = "REEVES, David",
                    Audited = false,
                    Completed = false,
                    Competition = "72721DT",
                    Schema = "abcdef01",
                    Number = 1,
                    Position = "Main",
                    AssistedBy = new List<string>() { "0002", "0003" }
                });
            }
            else
            {
                try
                {
                    AllEntries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataEntry>>(existingData);
                    if(AllEntries.FindAll(e => e.Id == "4a4a4a01").Count < 1)
                    {
                        AllEntries.Add(new DataEntry()
                        {
                            Id = "4a4a4a01",
                            TeamIdentifier = "0001",
                            TeamName = "REEVES, David",
                            Audited = false,
                            Completed = false,
                            Competition = "72721DT",
                            Schema = "abcdef01",
                            Number = 1,
                            Position = "Main",
                            AssistedBy = new List<string>() { "0002", "0003" }
                        });

                    }
                    else
                    {
                        AllEntries.RemoveAll(e => e.Id == "4a4a4a01");
                        AllEntries.Add(new DataEntry()
                        {
                            Id = "4a4a4a01",
                            TeamIdentifier = "0001",
                            TeamName = "REEVES, David",
                            Audited = false,
                            Completed = false,
                            Competition = "72721DT",
                            Schema = "abcdef01",
                            Number = 1,
                            Position = "Main",
                            AssistedBy = new List<string>() { "0002", "0003" }
                        });
                    }
                }
                catch (Exception e)
                {
                    AllEntries = new List<DataEntry>();
                }
            }
            await GetAvailableMatches();
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
            Users.Clear();
            Users.Add(new Scouter()
            {
                Name = "Gigawatt",
                Score = 0,
                Id = "uwuowo123",
                Banned = false,
                LastUsed = DateTime.Now
            });
            existingData = await StorageManager.GetData("competitions");
            if (existingData == "" || existingData == null)
            {
                Competitions = new List<Competition>();

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
                    Id = "abcdef01",
                    GotAt = DateTime.Now,
                    UsedFor = "Rapid React",
                    Name = "Rapid React",
                    JSONData = TestSchemaString
                });


            }
            else
            {
                try
                {
                    Schemas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schema>>(existingData);
                    if (Schemas.FindAll(s => s.Id == "abcdef01").Count < 1)
                    {
                        Schemas.Add(new Schema()
                        {
                            Id = "abcdef01",
                            GotAt = DateTime.Now,
                            UsedFor = "Rapid React",
                            Name = "Rapid React",
                            JSONData = TestSchemaString
                        });
                    }

                }
                catch (Exception e)
                {
                    Schemas = new List<Schema>();
                    Schemas.Add(new Schema()
                    {
                        Id = "abcdef01",
                        GotAt = DateTime.Now,
                        UsedFor = "Rapid React",
                        Name = "Rapid React",
                        JSONData = TestSchemaString
                    });
                }
            }
            if (CurrentApplicationData.DeviceId == null || CurrentApplicationData.DeviceId == "")
            {
                await GenerateFirstId();
            }
            else
            {
                //await GenerateFirstId();
            }
            existingData = await StorageManager.GetData("logs");
            if (existingData == "" || existingData == null)
            {
                Logs = new List<Log>();

            }
            else
            {
                try
                {
                    Logs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Log>>(existingData);

                }
                catch (Exception e)
                {
                    Logs = new List<Log>();
                }
            }
        }


        public async Task SetBackup()
        {
            var objToUse = new Backup() { Competitions = Competitions, CreatedAt = DateTime.Now, Entries = AllEntries, Schemas = Schemas };
            await StorageManager.SetData("backup", JsonConvert.SerializeObject(objToUse));
        }

        public async Task GetBackup()
        {
            var objToUse = JsonConvert.DeserializeObject<Backup>(await StorageManager.GetData("backup"));
            Competitions = objToUse.Competitions;
            await SaveCompetitions();
            AllEntries = objToUse.Entries;
            await SaveMatches();
            Schemas = objToUse.Schemas;
            await SaveSchemas();
        }

        public async Task GetAvailableMatches()
        {
            if (CurrentApplicationData.RestrictMatches)
            {
                AvailableEntries = AllEntries.FindAll(m => m.Competition == CurrentApplicationData.CurrentCompetition);
            }
            else
            {
                AvailableEntries = AllEntries;
            }
        }
        public async Task<bool> ClearAllData()
        {
            if (CurrentApplicationData.Debugging)
            {
                // WARNING: TO BE ONLY USED IN DEBUG MODE
                await StorageManager.SetData("app_data", "");
                await StorageManager.SetData("users", "");
                await StorageManager.SetData("schemas", "");
                await StorageManager.SetData("competitions", "");
                await StorageManager.SetData("matches", "");
                //await StorageManager.SetData("logs", "");
                return true;
            }
            return false;

        }
        public async Task<bool> AddLog(Log log)
        {
            if (CurrentApplicationData.Logging)
            {
                Logs.Add(log);
                SaveLogs();
                return true;
            }
            return false;
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
        public async Task SaveLogs()
        {
            var dataToPut = Newtonsoft.Json.JsonConvert.SerializeObject(Logs);
            await StorageManager.SetData("logs", dataToPut);
        }
        public async Task GenerateFirstId()
        {
            // only runs if there isn't an ID or the previous one was corrupt!
            char[] _base62chars =
                "0123456789ABCDEFG"
                .ToCharArray();
            Random randomGen = new Random();
            var finalString = "";
            for (int i = 0; i < 6; i++)
            {
                finalString = finalString + _base62chars[randomGen.Next(16)];
            }
            CurrentApplicationData.DeviceId = finalString;
            //CurrentApplicationData.DeviceId = "BF8613";
            await SaveAppData();
        }
    }
    public class ApplicationData
    {
        public string DeviceId;
        public Announcement CurrentAnnouncement;
        public string AdminCode;
        public string CurrentCompetition;
        public string EncryptionKey;
        public bool Locked;
        public string LockedMessage;
        public bool RestrictMatches;
        public bool Logging;
        public bool Debugging;
        public string SecurityKey;
        public bool SecurityMode;
    }
    public class Announcement
    {
        public string Title;
        public string Data;
        public DateTime GotAt;
        public DateTime ActiveUntil;
    }
    public class DiagnosticReport
    {
        public bool InternetConnectivity;
        public float BatteryLevel;
        public int NumberOfMatchesStored;
        public List<string> SchemasIncluded;
    }
    public class Log
    {
        public DateTime occured;
        public string eventType;
        public bool critical;
    }
    public class Scouter
    {
        public string Name { get; set; }
        [JsonProperty("_id")]
        public string Id { get; set; }
        public int Score { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Banned { get; set; }
    }
    public class Competition
    {
        [JsonProperty("prettyName")]
        public string Name;
        [JsonProperty("location")]
        public string Location;
        [JsonProperty("_id")]
        public string Id;
        [JsonProperty("acceptedSchemas")]
        public List<string> AllowedSchemas;
        public DateTime StartsAt;
        [JsonProperty("datespan")]
        public string DateSpan;
    }
    public class Schema
    {
        [JsonProperty("prettyName")]
        public string Name;
        [JsonProperty("usedFor")]
        public string UsedFor;
        [JsonProperty("_id")]
        public string Id;
        [JsonProperty("data")]
        public string JSONData;
        [JsonProperty("createdAt")]
        public DateTime GotAt;
    }
    public class DataEntry
    {
        [JsonProperty("_id")]
        public string Id;
        public string Competition;
        public string Schema;
        public int Number;
        public string Position;
        public MatchData Data;
        public DateTime LastEdited;
        public string TeamIdentifier;
        public string TeamName;
        public bool Completed;
        public bool Audited;
        public List<string> AssistedBy;
    }
    public class ActionLog
    {
        public string EventName;
        public string EventDetails;
        public DateTime GotAt;
    }
    public class Backup
    {
        public List<DataEntry> Entries;
        public List<Schema> Schemas;
        public List<Competition> Competitions;
        public DateTime CreatedAt;
    }
    public class MedicalInformation
    {
        public string Title;
        public string PrimaryContact;
        public string EmergencyPhone;
        public string Details;
    }
}
