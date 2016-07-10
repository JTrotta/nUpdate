﻿// Author: Dominic Beger (Trade/ProgTrade) 2016

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nUpdate.Administration.Logging;
using nUpdate.Administration.Sql;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace nUpdate.Administration.Application
{
    /// <summary>
    ///     Represents a local update project.
    /// </summary>
    [Serializable]
    internal class UpdateProject : PropertyChangedBase
    {
        private int _applicationId;
        private string _assemblyVersionPath;
        private Guid _guid;
        private List<PackageActionLogData> _logData;
        private string _name;
        private List<UpdatePackage> _packages;
        private string _privateKey;
        private TransferProtocol _transferProtocol;
        private ProxyData _proxyData;
        private string _publicKey;
        private SqlData _sqlData;
        private string _transferAssemblyFilePath;
        private ITransferData _transferData;
        private Uri _updateDirectoryUri;
        private bool _useProxy;
        private bool _useStatistics;

        public string ConfigVersion => "4";

        /// <summary>
        ///     Gets or sets the name of the project.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="System.Guid" /> of the project.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the application ID of the project for the statistics entries.
        /// </summary>
        public int ApplicationID
        {
            get { return _applicationId; }
            set
            {
                _applicationId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="System.Uri" /> of the remote update directory of the project.
        /// </summary>
        public Uri UpdateDirectoryUri
        {
            get { return _updateDirectoryUri; }
            set
            {
                _updateDirectoryUri = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the private key of the project for signing update packages.
        /// </summary>
        public string PrivateKey
        {
            get { return _privateKey; }
            set
            {
                _privateKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the public key of the project.
        /// </summary>
        public string PublicKey
        {
            get { return _publicKey; }
            set
            {
                _publicKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the path of the file containing the <see cref="System.Reflection.Assembly" /> of the .NET project that
        ///     should be updated.
        /// </summary>
        public string AssemblyVersionPath
        {
            get { return _assemblyVersionPath; }
            set
            {
                _assemblyVersionPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="ITransferData" /> that carries the necessary information for data transfers.
        /// </summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public ITransferData TransferData
        {
            get { return _transferData; }
            set
            {
                _transferData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the path of the file containing an assembly that implements a custom transfer protocol.
        /// </summary>
        public string TransferAssemblyFilePath
        {
            get { return _transferAssemblyFilePath; }
            set
            {
                _transferAssemblyFilePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Administration.TransferProtocol" /> that should be used for data transfers.
        /// </summary>
        public TransferProtocol TransferProtocol
        {
            get { return _transferProtocol; }
            set
            {
                _transferProtocol = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="PackageActionLogData" /> that carries information about the package history.
        /// </summary>
        public List<PackageActionLogData> LogData
        {
            get { return _logData; }
            set
            {
                _logData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the available <see cref="UpdatePackage" />s of the project.
        /// </summary>
        public List<UpdatePackage> Packages
        {
            get { return _packages; }
            set
            {
                _packages = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a statistics server should be used for the project, or not.
        /// </summary>
        public bool UseStatistics
        {
            get { return _useStatistics; }
            set
            {
                _useStatistics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Sql.SqlData" /> that carries the necessary information for statistics entries.
        /// </summary>
        public SqlData SqlData
        {
            get { return _sqlData; }
            set
            {
                _sqlData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a proxy should be used for data transfers, or not.
        /// </summary>
        public bool UseProxy
        {
            get { return _useProxy; }
            set
            {
                _useProxy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Administration.ProxyData" /> that carries the necessary information for proxies.
        /// </summary>
        public ProxyData ProxyData
        {
            get { return _proxyData; }
            set
            {
                _proxyData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Loads an update project from the specified path.
        /// </summary>
        /// <param name="path">The path of the project file.</param>
        /// <returns>The loaded <see cref="UpdateProject" />.</returns>
        public static UpdateProject Load(string path)
        {
            var updateProject = Serializer.Deserialize<UpdateProject>(File.ReadAllText(path));
            var currentProjectEntry = Session.AvailableLocations.FirstOrDefault(item => item.Guid == updateProject.Guid);
            if (currentProjectEntry == null)
                Session.AvailableLocations.Add(new UpdateProjectLocation(updateProject.Guid, path));
            else
            {
                if (currentProjectEntry.LastSeenPath != path)
                    currentProjectEntry.LastSeenPath = path;
            }

            return updateProject;
        }

        /// <summary>
        ///     Saves the current <see cref="UpdateProject" />.
        /// </summary>
        public void Save()
        {
            var updateProjectLocation = Session.AvailableLocations.FirstOrDefault(loc => loc.Guid == Guid);
            if (updateProjectLocation != null)
                File.WriteAllText(updateProjectLocation.LastSeenPath, Serializer.Serialize(this));
        }

        /// <summary>
        ///     Saves the current <see cref="UpdateProject" />.
        /// </summary>
        public void Save(string path)
        {
            File.WriteAllText(path, Serializer.Serialize(this));
        }
    }
}