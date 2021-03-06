﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace DGTools {
    [Serializable]
    public class PackageDatabase
    {
        //VARIABLES
        [SerializeField] List<Package> packages = new List<Package>();

        //PROPERTIES
        public static string packagesDirectoryPath {
            get {
                return Directory.GetParent(Application.dataPath) + "/Packages";
            }
        }

        public static string corePackagePath {
            get
            {
                return packagesDirectoryPath + "/com.dgtools.core";
            }
        }

        public static string databasePath {
            get {
                return corePackagePath + "/Editor/Resources/packageDatabase.json" ;
            }
        }

        public static bool isDevelopement {get; set;}

        //METHODS
        public static PackageDatabase Load() {
            if (!File.Exists(databasePath)) {
                throw new FileNotFoundException("Package Database file not found at " + databasePath + " (if deleted you can create a new one at this path)");
            }

            string json = File.ReadAllText(databasePath);

            PackageDatabase database = JsonUtility.FromJson<PackageDatabase>(json);
            JToken lockToken = LoadManifest().SelectToken("inDevelopement");
            isDevelopement = !(lockToken == null || (bool)lockToken == false);
            return database;
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(this);

            File.WriteAllText(databasePath, json);
        }

        public static JObject LoadManifest()
        {
            string manifestPath = packagesDirectoryPath + "/manifest.json";
            if (File.Exists(manifestPath))
            {
                return JObject.Parse(File.ReadAllText(manifestPath));
            }

            throw new FileNotFoundException("Impossible to load manifest at path : " + manifestPath);
        }

        public static void SaveManifest(JObject manifest) {
            string manifestPath = packagesDirectoryPath + "/manifest.json";
            string json = manifest.ToString();

            File.WriteAllText(manifestPath, json);
        }     

        public void AddPackage(Package package) {
            packages.Add(package);
        }

        public void DeletePackage(Package package) {
            if (packages.Contains(package)) {
                packages.Remove(package);
            }
        }

        public void ImportPackage(Package package, string version) {
            JObject manifest = LoadManifest();
            manifest["dependencies"][package.name] = (package.isLocal ? "file:" : "") + package.remotePath + "#" + version;

            SaveManifest(manifest);
        }

        public void RemovePackage(Package package) {
            if (isDevelopement)
                throw new Exception("Package data base in developpement :  deletion canceled");

            JObject manifest = LoadManifest();
            ((JObject)manifest.SelectToken("dependencies")).Remove(package.name);

            SaveManifest(manifest);
        }

        public List<Package> GetPackages() {
            return packages;
        }
    }
}

