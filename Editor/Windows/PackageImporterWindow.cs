﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.EditorCoroutines.Editor;

namespace DGTools.Editor {
    public class PackageImporterWindow : DGToolsWindow
    {
        //SETTINGS
        const string scrollBackgroundColor = "#EEEEEE";
        const int lineHeight = 30;
        const float nameColumnWidth = 0.35f; //In percent [0,1]
        const float versionColumnWidth = 0.20f; //In percent [0,1]
        const float statusColumnWidth = 0.15f; //In percent[0, 1]
        const float actionColumnWidth = 0.25f; //In percent[0, 1]
        const float editionWindowHeight = 100;

        //VARIABLES
        PackageDatabase packageDatabase;
        Vector2 scrollPosition = Vector2.zero;
        SortMode sortMode;
        Filter filter;
        bool editionWindowOpened = false;
        Package editedPackage;

        //ENUMS
        enum SortMode { AZ, Time}
        enum Filter { All, Installed, Available}

        //EDITOR METHODS
        [MenuItem("DGTools/Package Importer")]
        public static void ShowWindow()
        {
            PackageImporterWindow window = GetWindow(typeof(PackageImporterWindow)) as PackageImporterWindow;
            window.titleContent = new GUIContent("Package Importer");
            window.ReloadPackages();           
        }

        private void OnGUI()
        {
            // Define GUIStyles
            GUIStyle labelStyle = skin.GetStyle("Label");
            GUIStyle arrayLineStyle = skin.GetStyle("ArrayLine");
            GUIStyle buttonStyle = skin.GetStyle("Button");
            GUIStyle arrayHeaderStyle = skin.GetStyle("ArrayHeaders");
            Color bgColor;
            ColorUtility.TryParseHtmlString(scrollBackgroundColor, out bgColor);

            // Top menu 
            Rect topMenu = new Rect(0, 0, position.width, 30);

            // Sort box
            Rect elementRect = new Rect(topMenu.xMin + 10, topMenu.yMin + 5, topMenu.width * 0.25f, topMenu.height - 10);
            GUI.Label(new Rect(elementRect.xMin, elementRect.y, elementRect.width * 0.35f, elementRect.height), "Sorting", labelStyle);
            sortMode = (SortMode)EditorGUI.EnumPopup(new Rect(elementRect.center.x, elementRect.y, elementRect.width * 0.65f, elementRect.height), sortMode);

            // Filter box
            elementRect = new Rect(elementRect.xMax + 50, elementRect.yMin, topMenu.width * 0.25f, topMenu.height - 10);
            GUI.Label(new Rect(elementRect.xMin, elementRect.y, elementRect.width * 0.35f, elementRect.height), "Filter", labelStyle);
            filter = (Filter)EditorGUI.EnumPopup(new Rect(elementRect.center.x, elementRect.y, elementRect.width * 0.65f, elementRect.height), filter);

            // New Button
            
            elementRect = new Rect(elementRect.xMax + topMenu.width * 0.1f, elementRect.yMin, topMenu.width * 0.25f, topMenu.height - 10);
            if (PackageDatabase.isDevelopement)
            {
                if (GUI.Button(elementRect, "New Package", buttonStyle))
                {
                    CreatePackage();
                }
            }
            else {
                if (GUI.Button(elementRect, "Refresh", buttonStyle))
                {
                    ReloadPackages();
                }
            }

            float yPos = topMenu.yMax;

            // Define scroll area
            Rect scrollRect = new Rect(0, yPos + lineHeight, position.width, position.height - yPos -lineHeight - (editionWindowOpened?editionWindowHeight:0));           
            EditorGUI.DrawRect(scrollRect, bgColor);

            // Draw array labels
            Rect line = new Rect(scrollRect.xMin, scrollRect.yMin - 30, scrollRect.width * nameColumnWidth, lineHeight);           
            GUI.Label(line, "Name", arrayHeaderStyle);

            line = new Rect(line.xMax, line.yMin, scrollRect.width * versionColumnWidth, line.height);
            GUI.Label(line, "Versions", arrayHeaderStyle);

            //line = new Rect(line.xMax, line.yMin, scrollRect.width * versionColumnWidth, line.height);
            //GUI.Label(line, "Last Modified", arrayHeaderStyle);

            line = new Rect(line.xMax, line.yMin, scrollRect.width * statusColumnWidth, line.height);
            GUI.Label(line, "Status", arrayHeaderStyle);

            line = new Rect(line.xMax, line.yMin, scrollRect.width * actionColumnWidth, line.height);
            GUI.Label(line, "Actions", arrayHeaderStyle);

            // Draw scroll area
            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, new Rect(0, yPos + lineHeight, scrollRect.width-20, packageDatabase.GetPackages().Count * lineHeight));            

            // Draw Array lines
            yPos = scrollRect.yMin;
            foreach (Package package in GetSortedPackages()) {
                bool available = package.hasLocalPath;
                if ((filter == Filter.Available && available) || (filter == Filter.Installed && !available)) continue;

                line = new Rect(scrollRect.xMin, yPos, scrollRect.width * nameColumnWidth, lineHeight);
                GUI.Label(line, package.name, arrayLineStyle);

                line = new Rect(line.xMax, line.yMin, scrollRect.width * versionColumnWidth, line.height);
                if (package.isLoaded) {
                    package.offset = EditorGUI.Popup(line, package.offset, package.availableVersions.ToArray());
                }
                else {
                    GUI.Label(line, "Loading...", arrayLineStyle);
                }

                //line = new Rect(line.xMax, line.yMin, scrollRect.width * versionColumnWidth, line.height);
                //GUI.Label(line, package.lastEdition.ToShortDateString(), arrayLineStyle);

                line = new Rect(line.xMax, line.yMin, scrollRect.width * statusColumnWidth, line.height);
                if (available)
                {
                    GUI.Label(line, "Installed", arrayLineStyle);
                    line = new Rect(line.xMax, line.yMin, scrollRect.width * actionColumnWidth * 0.6f, line.height);

                    if (package.offset != package.currentVersionOffset)
                    {
                        if (GUI.Button(line, "Update", buttonStyle))
                        {
                            UpdatePackage(package);
                        }
                    }
                    else {
                        if (GUI.Button(line, "Remove", buttonStyle))
                        {
                            RemovePackage(package);
                        }
                    }
                }
                else {
                    GUI.Label(line, "Available", arrayLineStyle);
                    line = new Rect(line.xMax, line.yMin, scrollRect.width * actionColumnWidth * 0.6f, line.height);
                    if (GUI.Button(line, "Import", buttonStyle))
                    {
                        ImportPackage(package);
                    }
                }     

                line = new Rect(line.xMax, line.yMin, scrollRect.width * actionColumnWidth * 0.4f, line.height);
                if (GUI.Button(line, "Edit", buttonStyle))
                {
                    EditPackage(package);
                }

                yPos += line.height;
            }

            GUI.EndScrollView();

            // Draw Edition window
            if (editionWindowOpened && editedPackage != null) {
                Rect editionContainer = new Rect(scrollRect.xMin, scrollRect.yMax, position.width, editionWindowHeight);

                if (GUI.Button(new Rect(editionContainer.xMax - 20, editionContainer.yMin + 4, 16, 16), "x", buttonStyle))
                {
                    CloseEditionWindow();
                    return;
                }
                editionContainer.xMax -= 24;

                line = new Rect(editionContainer.xMin, editionContainer.yMin, editionContainer.width, 20);
                yPos = line.yMin;
                GUI.Label(new Rect(line.xMin, yPos, line.width * 0.3f, line.height), "Name", labelStyle);
                editedPackage.name = EditorGUI.TextField(new Rect(line.xMin + line.width * 0.3f, yPos, line.width * 0.7f, line.height), editedPackage.name);
                yPos += line.height;

                GUI.Label(new Rect(line.xMin, yPos, line.width * 0.3f, line.height), "Path", labelStyle);
                if (!string.IsNullOrEmpty(editedPackage.remotePath)) editedPackage.remotePath = editedPackage.remotePath.Replace("\\", "/");
                editedPackage.remotePath = EditorGUI.TextField(new Rect(line.xMin + line.width * 0.3f, yPos, line.width * 0.5f, line.height), editedPackage.remotePath);
                GUI.Label(new Rect(line.xMin + line.width * 0.8f, yPos, line.width * 0.2f, line.height), editedPackage.isValidRemotePath ? "Valid" : "Invalid", labelStyle);
                yPos += line.height;

                GUI.Label(new Rect(line.xMin, yPos, line.width * 0.3f, line.height), "Is Local", labelStyle);
                editedPackage.isLocal = EditorGUI.Toggle(new Rect(line.xMin + line.width * 0.3f, yPos, line.width * 0.7f, line.height), editedPackage.isLocal);
                yPos += line.height;

                if (GUI.Button(new Rect(line.xMin + line.width * 0.2f, yPos, line.width * 0.3f, line.height), "Save", buttonStyle))
                {
                    SavePackage();
                }                

                if (GUI.Button(new Rect(line.xMin + line.width * 0.5f, yPos, line.width * 0.3f, line.height), "Delete", buttonStyle))
                {
                    DeletePackage(editedPackage);
                }
            }           
        }

        //METHODS
        void ReloadPackages() {
            packageDatabase = PackageDatabase.Load();
            EditorCoroutineUtility.StartCoroutine(LoadVersions(), this);
        }

        void ImportPackage(Package package) {
            packageDatabase.ImportPackage(package);
            AssetDatabase.Refresh();
        }

        void RemovePackage(Package package) {
            packageDatabase.RemovePackage(package);
            AssetDatabase.Refresh();
        }

        void UpdatePackage(Package package)
        {
            package.currentVersionOffset = package.offset;
            ImportPackage(package);
            ReloadPackages();
        }

        void EditPackage(Package package) {
            editionWindowOpened = true;
            editedPackage = package;
        }

        void CreatePackage() {
            editionWindowOpened = true;
            editedPackage = new Package();
        }

        void SavePackage() {
            if (!packageDatabase.GetPackages().Contains(editedPackage)) {
                packageDatabase.AddPackage(editedPackage);
            }

            packageDatabase.Save();
            ReloadPackages();
            CloseEditionWindow();
        }

        void DeletePackage(Package package) {
            packageDatabase.DeletePackage(package);
            packageDatabase.Save();
            ReloadPackages();
            CloseEditionWindow();
        }

        void CloseEditionWindow() {
            editedPackage = null;
            editionWindowOpened = false;
            ReloadPackages();
        }

        List<Package> GetSortedPackages() {
            List<Package> packages = packageDatabase.GetPackages();

            if (sortMode == SortMode.AZ)
            {
                packages = packages.OrderBy(p => p.name).ToList();
            }
            else if (sortMode == SortMode.Time) {
                packages = packages.OrderBy(p => p.lastEdition).ToList();
            }

            return packages;
        }

        //COROUTINES
        IEnumerator LoadVersions() {
            EditorCoroutine coroutine;
            foreach (Package package in packageDatabase.GetPackages()) {
                coroutine = EditorCoroutineUtility.StartCoroutine(package.LoadAvailableVersions(), this);
                while (!package.isErrored && !package.isLoaded) {
                    yield return null;
                }
            }
        }
    }
}

