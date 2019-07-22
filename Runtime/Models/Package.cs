﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace DGTools{
    [Serializable]
    public class Package
    {
        //VARIABLES
        public string name;
        public string remotePath = "";
        public bool isLocal;
        

        [NonSerialized] public int offset;

        //PROPERTIES
        public bool hasLocalPath
        {
            get
            {
                return Directory.Exists(localPath);
            }
        }

        public string localPath
        {
            get
            {
                return Directory.GetParent(Application.dataPath) + "/Packages/" + name;
            }
        }

        public JObject infos
        {
            get
            {
                if (hasLocalPath)
                {
                    return JObject.Parse(File.ReadAllText(localPath));
                }
                return null;
            }
        }

        public DateTime lastEdition
        {
            get
            {
                if (isValidRemotePath)
                {
                    if (isLocal)
                    {
                        return File.GetLastWriteTime(remotePath + "/package.json");
                    }
                }
                return DateTime.Now;
            }
        }

        public bool isValidRemotePath
        {
            get
            {
                if (isLocal)
                {
                    return File.Exists(remotePath + "/package.json");
                }
                else
                {
                    return remotePath.Contains(".git");
                }
            }

        }
        public string currentVersion {
            get { return availableVersions[currentVersionOffset]; }
        }

        public List<string> availableVersions { get; private set; }

        public int currentVersionOffset { get; set; }

        public bool isLoaded
        {
            get
            {
                return !isLocal && (availableVersions != null && availableVersions.Count > 0);
            }
        }

        public bool isErrored { get; private set; }

        //COROUTINES
        public IEnumerator LoadAvailableVersions()
        {
            if (isLocal) yield break;

            isErrored = false;
            availableVersions = new List<string>();

            string request = remotePath.Replace(".git", "/tags");
            request = request.Replace("github.com", "api.github.com/repos");

            UnityWebRequest www = UnityWebRequest.Get(request);
            yield return www.SendWebRequest();
            while (!www.isDone)
            {
                if (www.isNetworkError || www.isHttpError)
                {
                    isErrored = true;
                    throw new Exception(www.error);
                }
                yield return null;
            }


            JArray tags = JArray.Parse(www.downloadHandler.text);
            foreach (JObject tag in tags)
            {
                availableVersions.Add((string)tag.SelectToken("name"));
            }

            if (PackageDatabase.isDevelopement)
            {
                request = request.Replace("tags", "branches");

                www = UnityWebRequest.Get(request);
                yield return www.SendWebRequest();
                while (!www.isDone)
                {
                    if (www.isNetworkError || www.isHttpError)
                    {
                        isErrored = true;
                        throw new Exception(www.error);
                    }
                    yield return null;
                }

                JArray branches = JArray.Parse(www.downloadHandler.text);
                foreach (JObject branch in branches)
                {
                    availableVersions.Add((string)branch.SelectToken("name"));
                }
            }

            string package = (string)PackageDatabase.LoadManifest()["dependencies"][name];

            if (package == null || !package.Contains("#")) yield break;

            string version = package.Split('#')[1];
            for (int i = 0; i < availableVersions.Count; i++)
            {
                if (availableVersions[i] == version)
                {
                    currentVersionOffset = i;
                    offset = i;
                }

            }
        }
    }
}
