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
        public async Task<bool> DeleteData(string fileName)
        {
            try
            {
                IFolder folder = await rootFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
                IFile file = await folder.GetFileAsync(fileName);
                await file.DeleteAsync();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
            
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
            //await StorageManager.DeleteData("matches");
            existingData = await StorageManager.GetData("matches");
            
            if(existingData == "" || existingData == null)
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
                    Schema = "444899fa",
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
                }catch(Exception e)
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
            if(CurrentApplicationData.DeviceId == null || CurrentApplicationData.DeviceId == "")
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
            for(int i = 0; i < 6; i++)
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
