using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using NLog;
using Oleg_ivo.MeloManager.MediaObjects.Extensions;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-контейнер
    /// </summary>
    [DebuggerDisplay("Медиа-контейнер [{Name}]")]
    public class MediaContainer : IRepairable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Родительские элементы
        /// </summary>
        public virtual ICollection<MediaContainer> ParentContainers { get; protected set; }
        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public virtual ICollection<MediaContainer> ChildContainers { get; protected set; }

        /// <summary>
        /// Добавляет дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        protected virtual void AddChild(MediaContainer child)
        {
            if (ChildContainers == null) ChildContainers = new List<MediaContainer>();
            ChildContainers.Add(child);
        }

        /// <summary>
        /// Добавляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        protected virtual void AddParent(MediaContainer parent)
        {
            if (ParentContainers == null) ParentContainers = new List<MediaContainer>();
            ParentContainers.Add(parent);
        }

        /// <summary>
        /// Удаляет дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        public virtual void RemoveChild(MediaContainer child)
        {
            ChildContainers.Remove(child);
        }

        /// <summary>
        /// Удаляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        public virtual void RemoveParent(MediaContainer parent)
        {
            ParentContainers.Remove(parent);
        }

        /// <summary>
        /// Перемещение между родительскими элементами
        /// </summary>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        protected virtual void MoveBetweenParents(MediaContainer oldParent, MediaContainer newParent)
        {
            newParent.AddChild(this);
            oldParent.RemoveChild(this);
        }

        /// <summary>
        /// Перемещение между дочерними элементами
        /// </summary>
        /// <param name="oldChild"></param>
        /// <param name="newChild"></param>
        protected virtual void MoveBetweenChildren(MediaContainer oldChild, MediaContainer newChild)
        {
            newChild.AddParent(this);
            oldChild.RemoveParent(this);
        }

        /// <summary>
        /// Проигрывает содержимое медиа-контейнера в программе-плейере
        /// </summary>
        public virtual void Play()
        {

        }

        /// <summary>
        /// Починка медиа-контейнера (рекурсивно)
        /// </summary>
        /// <param name="foundFiles"></param>
        /// <param name="optionRepairOnlyBadFiles"></param>
        /// <param name="mediaCache"></param>
        public virtual void BatchRepair(IEnumerable<string> foundFiles, bool optionRepairOnlyBadFiles, IMediaCache mediaCache)
        {
            IsRepaired = false;
            var foundFilesList = foundFiles as IList<string> ?? foundFiles.ToList();
            foreach (var mediaContainer in ChildContainers)
            {
                if (mediaContainer is Category || mediaContainer is Playlist)
                    log.Trace("Починка {0}", mediaContainer);
                mediaContainer.BatchRepair(foundFilesList, optionRepairOnlyBadFiles, mediaCache);
                if (!IsRepaired)
                    IsRepaired = mediaContainer.IsRepaired;
            }
/*
            var result = Parallel.ForEach(Children, mediaContainer =>
            {
                if (mediaContainer is Category || mediaContainer is Playlist)
                    log.Trace("Починка {0}", mediaContainer);
                mediaContainer.BatchRepair(foundFilesList, optionRepairOnlyBadFiles);
                if (!IsRepaired)
                    IsRepaired = mediaContainer.IsRepaired;

            });
            if (!result.IsCompleted)
            {
                
            }
*/
        }

//TODO: INPC?

        public MediaContainer()
        {
            //Files = new HashSet<File>();
            //ChildContainers = (childMediaContainers = new ObservableCollection<MediaContainer>());
            //ParentContainers = (parentMediaContainers = new ObservableCollection<MediaContainer>());
            
            //childMediaContainers.CollectionChanged += childMediaContainers_CollectionChanged;
            //parentMediaContainers.CollectionChanged += parentMediaContainers_CollectionChanged;
        }

        public virtual bool IsRepaired { get; set; }
        public virtual ICollection<File> Files { get; set; }
        public virtual long Id { get; set; }
        public virtual Guid RowGuid { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsRoot { get; set; }
        public virtual DateTime? DateInsert { get; set; }
        public virtual DateTime? DateUpdate { get; set; }

        private readonly ObservableCollection<MediaContainer> childMediaContainers;
        private readonly ObservableCollection<MediaContainer> parentMediaContainers;


        protected virtual void childMediaContainers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                InvokeChildrenChanged(e.Action, e.GetChange<MediaContainer>());
        }

        protected virtual void parentMediaContainers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                InvokeParentsChanged(e.Action, e.GetChange<MediaContainer>());
        }

        protected virtual void InvokeChildrenChanged(NotifyCollectionChangedAction listChangedType, MediaContainer child)
        {
            if (ChildrenChanged != null)
                ChildrenChanged(this, new MediaListChangedEventArgs(listChangedType, child));
        }

        protected virtual void InvokeParentsChanged(NotifyCollectionChangedAction listChangedType, MediaContainer child)
        {
            if (ParentsChanged != null)
                ParentsChanged(this, new MediaListChangedEventArgs(listChangedType, child));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual event EventHandler<MediaListChangedEventArgs> ChildrenChanged;
        public virtual event EventHandler<MediaListChangedEventArgs> ParentsChanged;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name;
        }

        public virtual Type GetRealType()
        {
            return ObjectContext.GetObjectType(GetType());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MediaListChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public NotifyCollectionChangedAction ListChangedType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainer MediaContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listChangedType"></param>
        /// <param name="mediaContainer"></param>
        public MediaListChangedEventArgs(NotifyCollectionChangedAction listChangedType, MediaContainer mediaContainer)
        {
            ListChangedType = listChangedType;
            MediaContainer = mediaContainer;
        }
    }
}