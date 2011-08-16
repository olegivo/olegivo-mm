using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-контейнер
    /// </summary>
    partial class MediaContainer
    {
        /// <summary>
        /// Дочерние элементы
        /// </summary>
        protected IQueryable<MediaContainer> Childs
        {
            get
            {
                return ChildMediaContainers.Select(mc => mc.ChildMediaContainer).AsQueryable();
            }
        }

        /// <summary>
        /// Родительские элементы
        /// </summary>
        protected IQueryable<MediaContainer> Parents
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
        protected void RemoveChild(MediaContainer child)
        {
            MediaContainersParentChild found =
                ChildMediaContainers.Where(mc => mc.ParentMediaContainer == this && mc.ChildMediaContainer == child).
                    FirstOrDefault();
            if (found != null)
                ChildMediaContainers.Remove(found);
        }

        /// <summary>
        /// Добавляет родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        protected void RemoveParent(MediaContainer parent)
        {
            MediaContainersParentChild found =
                ChildMediaContainers.Where(mc => mc.ChildMediaContainer == this && mc.ParentMediaContainer == parent).
                    FirstOrDefault();
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
        protected void MoveBetweenChilds(MediaContainer oldChild, MediaContainer newChild)
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
        public virtual void BatchRepair(string[] foundFiles, bool optionRepairOnlyBadFiles)
        {
            IsRepaired = false;
            if (foundFiles.Length > 0)
                foreach (MediaContainer mediaContainer in Childs)
                {
                    mediaContainer.BatchRepair(foundFiles, optionRepairOnlyBadFiles);
                    if (!IsRepaired)
                        IsRepaired = mediaContainer.IsRepaired;
                }
        }
    }
}