using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApiCore.FirestoreDataAccess
{
    public class FirestoreDataAccessLayer
    {
        string projectId;
        public FirestoreDb fireStoreDb;
        public FirestoreDataAccessLayer()
        {
            string filepath = AppDomain.CurrentDomain.BaseDirectory + @"googlegserviceaccount\male-290100-b0b617742938.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filepath);
            projectId = "male-e6ded";
            fireStoreDb = FirestoreDb.Create(projectId);
        }
    }
}
