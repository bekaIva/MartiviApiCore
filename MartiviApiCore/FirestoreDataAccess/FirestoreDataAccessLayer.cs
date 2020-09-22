using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.FirestoreDataAccess
{
    public class FirestoreDataAccessLayer
    {
        string projectId;
        public FirestoreDb fireStoreDb;
        public FirestoreDataAccessLayer()
        {
            string filepath = AppDomain.CurrentDomain.BaseDirectory + @"googlegserviceaccount\martivi-ffa6f-00e5a2503ce8.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filepath);
            projectId = "martivi-ffa6f";
            fireStoreDb = FirestoreDb.Create(projectId);
        }
    }
}
