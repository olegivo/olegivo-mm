﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Oleg_ivo.MeloManager.MediaObjects
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="MeloManager")]
	public partial class MediaDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertMediaContainer(MediaContainer instance);
    partial void UpdateMediaContainer(MediaContainer instance);
    partial void DeleteMediaContainer(MediaContainer instance);
    partial void InsertFile(File instance);
    partial void UpdateFile(File instance);
    partial void DeleteFile(File instance);
    partial void InsertMediaContainerFile(MediaContainerFile instance);
    partial void UpdateMediaContainerFile(MediaContainerFile instance);
    partial void DeleteMediaContainerFile(MediaContainerFile instance);
    partial void InsertMediaContainersParentChild(MediaContainersParentChild instance);
    partial void UpdateMediaContainersParentChild(MediaContainersParentChild instance);
    partial void DeleteMediaContainersParentChild(MediaContainersParentChild instance);
    #endregion
		
		public MediaDataContext() : 
				base(global::Oleg_ivo.MeloManager.MediaObjects.Properties.Settings.Default.MeloManagerConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public MediaDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MediaDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MediaDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MediaDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<MediaContainer> MediaContainers
		{
			get
			{
				return this.GetTable<MediaContainer>();
			}
		}
		
		public System.Data.Linq.Table<File> Files
		{
			get
			{
				return this.GetTable<File>();
			}
		}
		
		public System.Data.Linq.Table<MediaContainerFile> MediaContainerFiles
		{
			get
			{
				return this.GetTable<MediaContainerFile>();
			}
		}
		
		public System.Data.Linq.Table<MediaContainersParentChild> MediaContainersParentChilds
		{
			get
			{
				return this.GetTable<MediaContainersParentChild>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MediaContainers")]
	[global::System.Data.Linq.Mapping.InheritanceMappingAttribute(Code="MediaContainer", Type=typeof(MediaContainer), IsDefault=true)]
	[global::System.Data.Linq.Mapping.InheritanceMappingAttribute(Code="Category", Type=typeof(Category))]
	[global::System.Data.Linq.Mapping.InheritanceMappingAttribute(Code="Playlist", Type=typeof(Playlist))]
	[global::System.Data.Linq.Mapping.InheritanceMappingAttribute(Code="MediaFile", Type=typeof(MediaFile))]
	public partial class MediaContainer : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id = default(long);
		
		private string _Type;
		
		private string _Name;
		
		private bool _IsRoot;
		
		private EntitySet<MediaContainerFile> _MediaContainerFiles;
		
		private EntitySet<MediaContainersParentChild> _ParentMediaContainers;
		
		private EntitySet<MediaContainersParentChild> _ChildMediaContainers;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnTypeChanging(string value);
    partial void OnTypeChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnIsRootChanging(bool value);
    partial void OnIsRootChanged();
    #endregion
		
		public MediaContainer()
		{
			this._MediaContainerFiles = new EntitySet<MediaContainerFile>(new Action<MediaContainerFile>(this.attach_MediaContainerFiles), new Action<MediaContainerFile>(this.detach_MediaContainerFiles));
			this._ParentMediaContainers = new EntitySet<MediaContainersParentChild>(new Action<MediaContainersParentChild>(this.attach_ParentMediaContainers), new Action<MediaContainersParentChild>(this.detach_ParentMediaContainers));
			this._ChildMediaContainers = new EntitySet<MediaContainersParentChild>(new Action<MediaContainersParentChild>(this.attach_ChildMediaContainers), new Action<MediaContainersParentChild>(this.detach_ChildMediaContainers));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public long Id
		{
			get
			{
				return this._Id;
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Type", DbType="VarChar(255) NOT NULL", CanBeNull=false, IsDiscriminator=true)]
		public string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				if ((this._Type != value))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._Type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsRoot", DbType="Bit NOT NULL")]
		public bool IsRoot
		{
			get
			{
				return this._IsRoot;
			}
			set
			{
				if ((this._IsRoot != value))
				{
					this.OnIsRootChanging(value);
					this.SendPropertyChanging();
					this._IsRoot = value;
					this.SendPropertyChanged("IsRoot");
					this.OnIsRootChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainerFile", Storage="_MediaContainerFiles", ThisKey="Id", OtherKey="MediaContainerId")]
		public EntitySet<MediaContainerFile> MediaContainerFiles
		{
			get
			{
				return this._MediaContainerFiles;
			}
			set
			{
				this._MediaContainerFiles.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainersParentChild", Storage="_ParentMediaContainers", ThisKey="Id", OtherKey="ChildId")]
		public EntitySet<MediaContainersParentChild> ParentMediaContainers
		{
			get
			{
				return this._ParentMediaContainers;
			}
			set
			{
				this._ParentMediaContainers.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainersParentChild1", Storage="_ChildMediaContainers", ThisKey="Id", OtherKey="ParentId")]
		public EntitySet<MediaContainersParentChild> ChildMediaContainers
		{
			get
			{
				return this._ChildMediaContainers;
			}
			set
			{
				this._ChildMediaContainers.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_MediaContainerFiles(MediaContainerFile entity)
		{
			this.SendPropertyChanging();
			entity.MediaContainer = this;
		}
		
		private void detach_MediaContainerFiles(MediaContainerFile entity)
		{
			this.SendPropertyChanging();
			entity.MediaContainer = null;
		}
		
		private void attach_ParentMediaContainers(MediaContainersParentChild entity)
		{
			this.SendPropertyChanging();
			entity.ChildMediaContainer = this;
		}
		
		private void detach_ParentMediaContainers(MediaContainersParentChild entity)
		{
			this.SendPropertyChanging();
			entity.ChildMediaContainer = null;
		}
		
		private void attach_ChildMediaContainers(MediaContainersParentChild entity)
		{
			this.SendPropertyChanging();
			entity.ParentMediaContainer = this;
		}
		
		private void detach_ChildMediaContainers(MediaContainersParentChild entity)
		{
			this.SendPropertyChanging();
			entity.ParentMediaContainer = null;
		}
	}
	
	public partial class Category : MediaContainer
	{
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    #endregion
		
		public Category()
		{
			OnCreated();
		}
	}
	
	public partial class Playlist : MediaContainer
	{
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    #endregion
		
		public Playlist()
		{
			OnCreated();
		}
	}
	
	public partial class MediaFile : MediaContainer
	{
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    #endregion
		
		public MediaFile()
		{
			OnCreated();
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Files")]
	public partial class File : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id = default(long);
		
		private string _Drive;
		
		private string _Path;
		
		private string _Filename;
		
		private string _Extention;
		
		private string _FullFileName;
		
		private string _FileNameWithoutExtension;
		
		private EntitySet<MediaContainerFile> _MediaContainerFiles;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnDriveChanging(string value);
    partial void OnDriveChanged();
    partial void OnPathChanging(string value);
    partial void OnPathChanged();
    partial void OnFilenameChanging(string value);
    partial void OnFilenameChanged();
    partial void OnExtentionChanging(string value);
    partial void OnExtentionChanged();
    partial void OnFullFileNameChanging(string value);
    partial void OnFullFileNameChanged();
    partial void OnFileNameWithoutExtensionChanging(string value);
    partial void OnFileNameWithoutExtensionChanged();
    #endregion
		
		public File()
		{
			this._MediaContainerFiles = new EntitySet<MediaContainerFile>(new Action<MediaContainerFile>(this.attach_MediaContainerFiles), new Action<MediaContainerFile>(this.detach_MediaContainerFiles));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public long Id
		{
			get
			{
				return this._Id;
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Drive", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string Drive
		{
			get
			{
				return this._Drive;
			}
			set
			{
				if ((this._Drive != value))
				{
					this.OnDriveChanging(value);
					this.SendPropertyChanging();
					this._Drive = value;
					this.SendPropertyChanged("Drive");
					this.OnDriveChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Path", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string Path
		{
			get
			{
				return this._Path;
			}
			set
			{
				if ((this._Path != value))
				{
					this.OnPathChanging(value);
					this.SendPropertyChanging();
					this._Path = value;
					this.SendPropertyChanged("Path");
					this.OnPathChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Filename", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string Filename
		{
			get
			{
				return this._Filename;
			}
			set
			{
				if ((this._Filename != value))
				{
					this.OnFilenameChanging(value);
					this.SendPropertyChanging();
					this._Filename = value;
					this.SendPropertyChanged("Filename");
					this.OnFilenameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Extention", DbType="VarChar(10) NOT NULL", CanBeNull=false)]
		public string Extention
		{
			get
			{
				return this._Extention;
			}
			set
			{
				if ((this._Extention != value))
				{
					this.OnExtentionChanging(value);
					this.SendPropertyChanging();
					this._Extention = value;
					this.SendPropertyChanged("Extention");
					this.OnExtentionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FullFileName", AutoSync=AutoSync.Always, DbType="VarChar(162) NOT NULL", CanBeNull=false, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public string FullFileName
		{
			get
			{
				return this._FullFileName;
			}
			set
			{
				if ((this._FullFileName != value))
				{
					this.OnFullFileNameChanging(value);
					this.SendPropertyChanging();
					this._FullFileName = value;
					this.SendPropertyChanged("FullFileName");
					this.OnFullFileNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FileNameWithoutExtension", DbType="VarChar(40) NOT NULL", CanBeNull=false)]
		public string FileNameWithoutExtension
		{
			get
			{
				return this._FileNameWithoutExtension;
			}
			set
			{
				if ((this._FileNameWithoutExtension != value))
				{
					this.OnFileNameWithoutExtensionChanging(value);
					this.SendPropertyChanging();
					this._FileNameWithoutExtension = value;
					this.SendPropertyChanged("FileNameWithoutExtension");
					this.OnFileNameWithoutExtensionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="File_MediaContainerFile", Storage="_MediaContainerFiles", ThisKey="Id", OtherKey="FileId")]
		public EntitySet<MediaContainerFile> MediaContainerFiles
		{
			get
			{
				return this._MediaContainerFiles;
			}
			set
			{
				this._MediaContainerFiles.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_MediaContainerFiles(MediaContainerFile entity)
		{
			this.SendPropertyChanging();
			entity.File = this;
		}
		
		private void detach_MediaContainerFiles(MediaContainerFile entity)
		{
			this.SendPropertyChanging();
			entity.File = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MediaContainerFiles")]
	public partial class MediaContainerFile : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id = default(long);
		
		private long _MediaContainerId;
		
		private long _FileId;
		
		private EntityRef<File> _File;
		
		private EntityRef<MediaContainer> _MediaContainer;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnMediaContainerIdChanging(long value);
    partial void OnMediaContainerIdChanged();
    partial void OnFileIdChanging(long value);
    partial void OnFileIdChanged();
    #endregion
		
		public MediaContainerFile()
		{
			this._File = default(EntityRef<File>);
			this._MediaContainer = default(EntityRef<MediaContainer>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public long Id
		{
			get
			{
				return this._Id;
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MediaContainerId", DbType="BigInt NOT NULL")]
		public long MediaContainerId
		{
			get
			{
				return this._MediaContainerId;
			}
			set
			{
				if ((this._MediaContainerId != value))
				{
					if (this._MediaContainer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnMediaContainerIdChanging(value);
					this.SendPropertyChanging();
					this._MediaContainerId = value;
					this.SendPropertyChanged("MediaContainerId");
					this.OnMediaContainerIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FileId", DbType="BigInt NOT NULL")]
		public long FileId
		{
			get
			{
				return this._FileId;
			}
			set
			{
				if ((this._FileId != value))
				{
					if (this._File.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnFileIdChanging(value);
					this.SendPropertyChanging();
					this._FileId = value;
					this.SendPropertyChanged("FileId");
					this.OnFileIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="File_MediaContainerFile", Storage="_File", ThisKey="FileId", OtherKey="Id", IsForeignKey=true)]
		public File File
		{
			get
			{
				return this._File.Entity;
			}
			set
			{
				File previousValue = this._File.Entity;
				if (((previousValue != value) 
							|| (this._File.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._File.Entity = null;
						previousValue.MediaContainerFiles.Remove(this);
					}
					this._File.Entity = value;
					if ((value != null))
					{
						value.MediaContainerFiles.Add(this);
						this._FileId = value.Id;
					}
					else
					{
						this._FileId = default(long);
					}
					this.SendPropertyChanged("File");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainerFile", Storage="_MediaContainer", ThisKey="MediaContainerId", OtherKey="Id", IsForeignKey=true)]
		public MediaContainer MediaContainer
		{
			get
			{
				return this._MediaContainer.Entity;
			}
			set
			{
				MediaContainer previousValue = this._MediaContainer.Entity;
				if (((previousValue != value) 
							|| (this._MediaContainer.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._MediaContainer.Entity = null;
						previousValue.MediaContainerFiles.Remove(this);
					}
					this._MediaContainer.Entity = value;
					if ((value != null))
					{
						value.MediaContainerFiles.Add(this);
						this._MediaContainerId = value.Id;
					}
					else
					{
						this._MediaContainerId = default(long);
					}
					this.SendPropertyChanged("MediaContainer");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MediaContainersParentChilds")]
	public partial class MediaContainersParentChild : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id = default(long);
		
		private long _ParentId;
		
		private long _ChildId;
		
		private EntityRef<MediaContainer> _ChildMediaContainer;
		
		private EntityRef<MediaContainer> _ParentMediaContainer;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnParentIdChanging(long value);
    partial void OnParentIdChanged();
    partial void OnChildIdChanging(long value);
    partial void OnChildIdChanged();
    #endregion
		
		public MediaContainersParentChild()
		{
			this._ChildMediaContainer = default(EntityRef<MediaContainer>);
			this._ParentMediaContainer = default(EntityRef<MediaContainer>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public long Id
		{
			get
			{
				return this._Id;
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentId", DbType="BigInt NOT NULL")]
		public long ParentId
		{
			get
			{
				return this._ParentId;
			}
			set
			{
				if ((this._ParentId != value))
				{
					if (this._ParentMediaContainer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnParentIdChanging(value);
					this.SendPropertyChanging();
					this._ParentId = value;
					this.SendPropertyChanged("ParentId");
					this.OnParentIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ChildId", DbType="BigInt NOT NULL")]
		public long ChildId
		{
			get
			{
				return this._ChildId;
			}
			set
			{
				if ((this._ChildId != value))
				{
					if (this._ChildMediaContainer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnChildIdChanging(value);
					this.SendPropertyChanging();
					this._ChildId = value;
					this.SendPropertyChanged("ChildId");
					this.OnChildIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainersParentChild", Storage="_ChildMediaContainer", ThisKey="ChildId", OtherKey="Id", IsForeignKey=true)]
		public MediaContainer ChildMediaContainer
		{
			get
			{
				return this._ChildMediaContainer.Entity;
			}
			set
			{
				MediaContainer previousValue = this._ChildMediaContainer.Entity;
				if (((previousValue != value) 
							|| (this._ChildMediaContainer.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ChildMediaContainer.Entity = null;
						previousValue.ParentMediaContainers.Remove(this);
					}
					this._ChildMediaContainer.Entity = value;
					if ((value != null))
					{
						value.ParentMediaContainers.Add(this);
						this._ChildId = value.Id;
					}
					else
					{
						this._ChildId = default(long);
					}
					this.SendPropertyChanged("ChildMediaContainer");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MediaContainer_MediaContainersParentChild1", Storage="_ParentMediaContainer", ThisKey="ParentId", OtherKey="Id", IsForeignKey=true)]
		public MediaContainer ParentMediaContainer
		{
			get
			{
				return this._ParentMediaContainer.Entity;
			}
			set
			{
				MediaContainer previousValue = this._ParentMediaContainer.Entity;
				if (((previousValue != value) 
							|| (this._ParentMediaContainer.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ParentMediaContainer.Entity = null;
						previousValue.ChildMediaContainers.Remove(this);
					}
					this._ParentMediaContainer.Entity = value;
					if ((value != null))
					{
						value.ChildMediaContainers.Add(this);
						this._ParentId = value.Id;
					}
					else
					{
						this._ParentId = default(long);
					}
					this.SendPropertyChanged("ParentMediaContainer");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
