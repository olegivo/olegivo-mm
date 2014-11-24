using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-контейнер
    /// </summary>
    [DebuggerDisplay("Медиа-контейнер [{Name}]")]
    partial class MediaContainer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public IQueryable<MediaContainer> Children
        {
            get
            {
                return ChildMediaContainers.Select(mc => mc.ChildMediaContainer).AsQueryable();
            }
        }

        /// <summary>
        /// Родительские элементы
        /// </summary>
        public IQueryable<MediaContainer> Parents
        {
            get
            {
                return ParentMediaContainers.Select(mc => mc.ParentMediaContainer).AsQueryable();
            }
        }

        /// <summary>
        /// Добавляет дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        protected void AddChild(MediaContainer child)
        {
            ChildMediaContainers.Add(new MediaContainersParentChild { ChildMediaContainer = child, ParentMediaContainer = this });
        }

        /// <summary>
        /// Добавляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        protected void AddParent(MediaContainer parent)
        {
            ParentMediaContainers.Add(new MediaContainersParentChild { ChildMediaContainer = this, ParentMediaContainer = parent });
        }

        /// <summary>
        /// Удаляет дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(MediaContainer child)
        {
            var found =
                ChildMediaContainers.FirstOrDefault(mc => mc.ParentMediaContainer == this && mc.ChildMediaContainer == child);
            if (found != null)
            {
                //BeginRemoveChild(found);
                ChildMediaContainers.Remove(found);
                EndRemoveChild(found);
            }
        }

        private void EndRemoveChild(MediaContainersParentChild child)
        {
            InvokeChildrenChanged(ListChangedType.ItemDeleted, child.ChildMediaContainer);
        }

/*
        private void BeginRemoveChild(MediaContainersParentChild mediaContainersParentChild)
        {
            
        }
*/

        /// <summary>
        /// Добавляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveParent(MediaContainer parent)
        {
            var found =
                ChildMediaContainers.FirstOrDefault(mc => mc.ChildMediaContainer == this && mc.ParentMediaContainer == parent);
            if (found != null)
                ChildMediaContainers.Remove(found);
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
            foreach (var mediaContainer in Children)
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

        public bool IsRepaired { get; set; }//TODO: INPC?

        partial void OnCreated()
        {
            ChildMediaContainers.ListChanged += ChildMediaContainers_ListChanged;
        }

        void ChildMediaContainers_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                MediaContainer child = ChildMediaContainers[e.NewIndex].ChildMediaContainer;
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
        /// Получить связь, 
        /// в которой данный <see cref="MediaContainer"/> выступает в качестве родительского, 
        /// а <paramref name="child"/> - в качестве дочернего элемента
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public MediaContainersParentChild GetChildRelation(MediaContainer child)
        {
            return ChildMediaContainers.SingleOrDefault(relation => relation.ChildId == child.Id);
        }

        /// <summary>
        /// Получить связь, 
        /// в которой данный <see cref="MediaContainer"/> выступает в качестве дочернего, 
        /// а <paramref name="parent"/> - в качестве родительского элемента
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public MediaContainersParentChild GetParentRelation(MediaContainer parent)
        {
            return ParentMediaContainers.SingleOrDefault(relation => relation.ParentId == parent.Id);
        }

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