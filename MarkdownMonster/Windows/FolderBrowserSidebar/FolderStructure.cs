﻿using System.IO;
using System.Linq;
using MarkdownMonster.Utilities;


namespace MarkdownMonster.Windows
{
	public class FolderStructure
	{
	    internal static AssociatedIcons icons = new AssociatedIcons();

		/// <summary>
		/// Gets a folder hierarchy
		/// </summary>
		/// <param name="baseFolder"></param>
		/// <param name="parentPathItem"></param>
		/// <param name="skipFolders"></param>
		/// <returns></returns>
		public PathItem GetFilesAndFolders(string baseFolder, PathItem parentPathItem = null, string skipFolders = ".git,node_modules,bower_components,packages,testresults,bin,obj", bool nonRecursive = false, string searchText = null)
		{
			if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder) )
				return new PathItem();

			PathItem activeItem;
            bool isRootFolder = false;

			if (parentPathItem == null)
			{
                isRootFolder = true;
                activeItem = new PathItem
                {
                    FullPath = baseFolder,
                    IsFolder = true
                };
			    if (mmApp.Configuration.FolderBrowser.ShowIcons)
			    {
			        activeItem.SetFolderIcon();			        
			    }

			    parentPathItem = activeItem;
			}
			else
			{
				activeItem = new PathItem { FullPath=baseFolder, IsFolder = true, Parent = parentPathItem};
			    if (mmApp.Configuration.FolderBrowser.ShowIcons)
			        activeItem.SetFolderIcon();

                parentPathItem.Files.Add(activeItem);
			}


            string[] folders = null;

			try
			{
				folders = Directory.GetDirectories(baseFolder);

			    if (!string.IsNullOrEmpty(searchText))
			        folders = folders.Where(s => Path.GetFileName(s.ToLower()).Contains(searchText.ToLower())).ToArray();
			}
			catch { }

			if (folders != null)
			{
				foreach (var folder in folders)
				{
					var name = System.IO.Path.GetFileName(folder);
					if (!string.IsNullOrEmpty(name))
					{
						if (name.StartsWith("."))
							continue;
						// skip folders
						if (("," + skipFolders + ",").Contains("," + name.ToLower() + ","))
							continue;
					}

				    if (!nonRecursive)				        
                        GetFilesAndFolders(folder, activeItem, skipFolders);
                    else
				    {
				        var folderPath = new PathItem
				        {
				            FullPath = folder,
				            IsFolder = true,
				            Parent = activeItem
				        };
				        if (mmApp.Configuration.FolderBrowser.ShowIcons)
				            folderPath.SetFolderIcon();
				        folderPath.Files.Add(PathItem.Empty);

                        activeItem.Files.Add(folderPath);
				    }
				}
			}

			string[] files = null;
			try
			{
				files = Directory.GetFiles(baseFolder);
			    if (!string.IsNullOrEmpty(searchText))
			        files = files.Where(s =>
                        Path.GetFileName(s.ToLower()).Contains(searchText.ToLower())).ToArray();
            }
            catch { }

		    if (folders == null && nonRecursive)
		    {
		        //foreach (var folder in folders)
		        //{

		        //}
		    }
			if (files != null)
			{
				foreach (var file in files)
				{
				    var item = new PathItem {FullPath = file, Parent = activeItem, IsFolder = false, IsFile = true};
				    if (mmApp.Configuration.FolderBrowser.ShowIcons)
				        item.Icon = icons.GetIconFromFile(file);
                    
				    activeItem.Files.Add(item);
				}
			}

		    if (activeItem.FullPath.Length > 5 && isRootFolder )
		    {
		        var parentFolder = new PathItem
		        {
		            IsFolder = true,
		            FullPath = ".."
		        };
		        parentFolder.SetFolderIcon();
		        activeItem.Files.Insert(0, parentFolder);
		    }


		    return activeItem;
		}

        /// <summary>
        /// Sets visibility of all items in the path item tree
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pathItem"></param>
	    public void SetSearchVisibility(string searchText, PathItem pathItem)
	    {
	        searchText = searchText?.ToLower();

            if (pathItem.Files.Count == 1 && pathItem.Files[0] == PathItem.Empty)
                return;

	        foreach (var pi in pathItem.Files)
	        {                
	            if (string.IsNullOrEmpty(searchText) || pi.FullPath == "..")
	            {
	                pi.IsVisible = true;
                    continue;
	            }

	            if (Path.GetFileName(pi.DisplayName).ToLower().Contains(searchText))
                    pi.IsVisible = true;
                else
                    pi.IsVisible = false;

	            if (pi.IsFolder)
	                SetSearchVisibility(searchText, pi);
	        }
            
	    }
	}
}
