﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-контейнер
    /// </summary>
    [DebuggerDisplay("Медиа-контейнер [{Name}]")]
    public class MediaContainer
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
        protected void AddChild(MediaContainer child)
        {
            ChildContainers.Add(child);
        }

        /// <summary>
        /// Добавляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        protected void AddParent(MediaContainer parent)
        {
            ParentContainers.Add(parent);
        }

        /// <summary>
        /// Удаляет дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(MediaContainer child)
        {
            if(ChildContainers.Remove(child))
                EndRemoveChild(child);
        }

        private void EndRemoveChild(MediaContainer child)
        {
            InvokeChildrenChanged(ListChangedType.ItemDeleted, child);
        }

/*
        private void BeginRemoveChild(MediaContainersParentChild mediaContainersParentChild)
        {
            
        }
*/

        /// <summary>
        /// Удаляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveParent(MediaContainer parent)
        {
            ParentContainers.Remove(parent);
        }

        /// <summary>
        /// Перемещение между родительскими элементами
        /// </summary>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        protected void MoveBetweenParents(MediaContainer oldParent, MediaContainer newParent)
        {
            newParent.AddChild(this);
            oldParent.RemoveChild(this);
        }

        /// <summary>
        /// Перемещение между дочерними элементами
        /// </summary>
        /// <param name="oldChild"></param>
        /// <param name="newChild"></param>
        protected void MoveBetweenChildren(MediaContainer oldChild, MediaContainer newChild)
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
            Files = new HashSet<File>();
            ChildMediaContainers = new BindingList<MediaContainer>();
            ChildContainers = ChildMediaContainers;

            ChildMediaContainers.ListChanged += ChildMediaContainers_ListChanged;
        }

        public bool IsRepaired { get; set; }
        public virtual ICollection<File> Files { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsRoot { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }

        private BindingList<MediaContainer> ChildMediaContainers { get; set; }


        void ChildMediaContainers_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                var child = ChildMediaContainers[e.NewIndex];
                InvokeChildrenChanged(e.ListChangedType, child);
            }                      
        }

        private void InvokeChildrenChanged(ListChangedType listChangedType, MediaContainer child)
        {
            if (ChildrenChanged != null)
                ChildrenChanged(this, new MediaListChangedEventArgs(listChangedType, child));
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MediaListChangedEventArgs> ChildrenChanged;

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

        public Type GetRealType()
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
        public ListChangedType ListChangedType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainer MediaContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listChangedType"></param>
        /// <param name="mediaContainer"></param>
        public MediaListChangedEventArgs(ListChangedType listChangedType, MediaContainer mediaContainer)
        {
            ListChangedType = listChangedType;
            MediaContainer = mediaContainer;
        }
    }
}