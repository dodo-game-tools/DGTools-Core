using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.EditorCoroutines.Editor;

namespace DGTools.Editor {
    public class PackageImporterWindow : DGToolsWindow
    {
        #region Variables
        PackageDatabase packageDatabase;
        Vector2 scrollPosition = Vector2.zero;
        SortMode sortMode;
        Filter filter;
        bool editionWindowOpened = false;
        Package editedPackage;
        #endregion

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
            #region Top Bar
            GUILayout.BeginHorizontal(skin.box);

            GUILayout.Label("Sort by", skin.FindStyle("Title"));
            sortMode = (SortMode)EditorGUILayout.EnumPopup(sortMode);

            GUILayout.Label("Filter by", skin.FindStyle("Title"));
            filter = (Filter)EditorGUILayout.EnumPopup(filter);          
            
            if (GUILayout.Button("Refresh", skin.button))
            {
                ReloadPackages();
            }

            if (PackageDatabase.isDevelopement)
            {
                if (GUILayout.Button("New Package", skin.button))
                {
                    CreatePackage();
                }
            }
            if (GUILayout.Button("DM:"+ (PackageDatabase.isDevelopement? "On" : "Off"), skin.button))
            {
                PackageDatabase.isDevelopement = !PackageDatabase.isDevelopement;
            }
            GUILayout.EndHorizontal();
            #endregion



            #region Array Headers
            GUILayout.BeginHorizontal(skin.box);

            float cellWidth = position.width / 4 - 5;
            GUILayout.Label("Name", skin.FindStyle("Title"), GUILayout.Width(cellWidth));
            GUILayout.Label("Versions", skin.FindStyle("Title"), GUILayout.Width(cellWidth));
            GUILayout.Label("Status", skin.FindStyle("Title"), GUILayout.Width(cellWidth));
            GUILayout.Label("Actions", skin.FindStyle("Title"), GUILayout.Width(cellWidth));

            GUILayout.EndHorizontal();
            #endregion

            #region Package List
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, skin.box);

            foreach (Package package in GetSortedPackages()) {
                bool available = package.hasLocalPath;
                if ((filter == Filter.Available && available) || (filter == Filter.Installed && !available)) continue;

                #region Array Line
                GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false), GUILayout.MaxHeight(30));

                GUILayout.Label(package.name, skin.FindStyle("ArrayLine"), GUILayout.Width(cellWidth));

                if (package.isLoaded)
                {
                    GUILayout.BeginHorizontal(skin.FindStyle("ArrayLine"), GUILayout.Width(cellWidth));
                    package.offset = EditorGUILayout.Popup(package.offset, package.availableVersions.ToArray());
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Loading...", skin.FindStyle("ArrayLine"), GUILayout.Width(cellWidth));
                }

                GUILayout.Label(available?"Installed" : "Available", skin.FindStyle("ArrayLine"), GUILayout.Width(cellWidth));

                #region Action Field
                GUILayout.BeginHorizontal(skin.FindStyle("ArrayLine"), GUILayout.Width(cellWidth));
                if (available)
                {
                    if (package.offset != package.currentVersionOffset)
                    {
                        if (GUILayout.Button("Update", skin.button))
                        {
                            UpdatePackage(package);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Remove", skin.button))
                        {
                            RemovePackage(package);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Import", skin.button))
                    {
                        ImportPackage(package);
                    }
                }

                if (PackageDatabase.isDevelopement) {
                    if (GUILayout.Button("Edit", skin.button))
                    {
                        EditPackage(package);
                    }
                }
                
                GUILayout.EndHorizontal();
                #endregion

                GUILayout.EndHorizontal();
                #endregion
            }


            GUILayout.EndScrollView();
            #endregion

            if (editionWindowOpened && editedPackage != null)
            {
                #region Edition Window
                GUILayout.BeginVertical(skin.box);

                #region Close Button
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("x", skin.box, GUILayout.Width(16)))
                {
                    CloseEditionWindow();
                    return;
                }
                GUILayout.EndHorizontal();
                #endregion

                #region Name Field
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(cellWidth));
                editedPackage.name = EditorGUILayout.TextField(editedPackage.name);
                GUILayout.EndHorizontal();
                #endregion

                #region Name Field
                GUILayout.BeginHorizontal();
                GUILayout.Label("Path", GUILayout.Width(cellWidth));
                if (!string.IsNullOrEmpty(editedPackage.remotePath)) editedPackage.remotePath = editedPackage.remotePath.Replace("\\", "/");
                editedPackage.remotePath = EditorGUILayout.TextField(editedPackage.remotePath);
                GUILayout.Label(editedPackage.isValidRemotePath ? "Valid" : "Invalid");
                GUILayout.EndHorizontal();
                #endregion

                #region Name Field
                GUILayout.BeginHorizontal();
                GUILayout.Label("Is Local", GUILayout.Width(cellWidth));
                editedPackage.isLocal = EditorGUILayout.Toggle(editedPackage.isLocal);
                GUILayout.EndHorizontal();
                #endregion

                #region Buttons
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save", skin.button))
                {
                    SavePackage();
                }

                if (GUILayout.Button("Delete", skin.button))
                {
                    DeletePackage(editedPackage);
                }
                GUILayout.EndHorizontal();
                #endregion

                GUILayout.EndVertical();
                #endregion
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

