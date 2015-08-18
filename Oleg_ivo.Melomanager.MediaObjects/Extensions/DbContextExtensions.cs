using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Base.Utils;

namespace Oleg_ivo.MeloManager.MediaObjects.Extensions
{
    public static class DbContextExtensions
    {
        public class DbContextLogHelper : StateHolder<Action<string>>
        {
            private readonly DbContext dbContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public DbContextLogHelper(DbContext dbContext, Action<string> logger = null)
                : base(() =>
                {
                    var old = dbContext.Database.Log;
                    dbContext.Database.Log = logger ?? Console.WriteLine;
                    return old;
                }, 
                oldState => dbContext.Database.Log = oldState)
            {
                this.dbContext = Enforce.ArgumentNotNull(dbContext, "dbContext");
            }

            public void LogChangesInfo()
            {
                dbContext.ChangeTracker.DetectChanges();
                var changeSet =
                    dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State != EntityState.Unchanged)
                        .GroupBy(entry => entry.State)
                        .Select(g => string.Format("{0} :{1}", g.Key, g.Count()))
                        .ToList();
                dbContext.Database.Log("Изменения для фиксации:");
                dbContext.Database.Log(changeSet.Any() ? changeSet.JoinToString("\n") : "Нет");
            }

            public void SubmitChanges()
            {
                LogChangesInfo();
                dbContext.SaveChanges();
            }

        }

        public static void ActionWithLog<TDataContext>(this TDataContext dataContext, Action<TDataContext> action, Action<string> logger = null) where TDataContext : DbContext
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                action(dataContext);
            }
        }

        public static void ActionWithLog(this DbContext dataContext, Action action, Action<string> logger = null)
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                action();
            }
        }

        public static TResult FuncWithLog<TResult>(this DbContext dataContext, Func<TResult> func, Action<string> logger = null)
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                return func();
            }
        }

        public static void SubmitChangesWithLog(this DbContext dataContext, Action<string> logger = null)
        {
            using (var helper = new DbContextLogHelper(dataContext, logger))
            {
                helper.SubmitChanges();
            }
        }

        public static bool HasChanges(this DbContext dataContext)
        {
            return dataContext.ChangeTracker.HasChanges();
        }



        ///////////////////////////////////////////////////////////////


        public static ObjectContext GetObjectContext(this DbContext context)
        {
            return ((IObjectContextAdapter)context).ObjectContext;
        }

        ///////////////////////////////////////////////////////////////

        public static ObjectStateManager GetObjectStateManager(this DbContext context)
        {
            return context.GetObjectContext().ObjectStateManager;
        }

        public static EntityState GetEntityState(this ObjectContext context, EntityKey key)
        {
            var entry = context.ObjectStateManager.GetObjectStateEntry(key);
            return entry.State;
        }

        public static string GetFullEntitySetName(this EntityKey key)
        {
            return key.EntityContainerName + "." + key.EntitySetName;
        }

        public static object GetEntityByKey(this ObjectContext context, EntityKey key)
        {
            return context.ObjectStateManager.GetObjectStateEntry(key).Entity;
        }

        public static IExtendedDataRecord UsableValues(this ObjectStateEntry entry)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Modified:
                    return entry.CurrentValues;
                case EntityState.Deleted:
                    return (IExtendedDataRecord)entry.OriginalValues;
                default:
                    throw new InvalidOperationException("This entity state should not exist.");
            }
        }

        public static EdmType EdmType(this ObjectStateEntry entry)
        {
            return entry.UsableValues().DataRecordInfo.RecordType.EdmType;
        }

        public static bool IsManyToMany(this AssociationType associationType)
        {
            return associationType.RelationshipEndMembers.All(endMember => endMember.RelationshipMultiplicity == RelationshipMultiplicity.Many);
        }

        public static bool IsRelationshipForKey(this ObjectStateEntry entry, EntityKey key)
        {
            if (entry.IsRelationship == false)
            {
                return false;
            }
            return ((EntityKey)entry.UsableValues()[0] == key) || ((EntityKey)entry.UsableValues()[1] == key);
        }

        public static EntityKey OtherEndKey(this ObjectStateEntry relationshipEntry, EntityKey thisEndKey)
        {
            Debug.Assert(relationshipEntry.IsRelationship);
            Debug.Assert(thisEndKey != null);

            if ((EntityKey)relationshipEntry.UsableValues()[0] == thisEndKey)
            {
                return (EntityKey)relationshipEntry.UsableValues()[1];
            }
            if ((EntityKey)relationshipEntry.UsableValues()[1] == thisEndKey)
            {
                return (EntityKey)relationshipEntry.UsableValues()[0];
            }
            
            throw new InvalidOperationException("Neither end of the relationship contains the passed in key.");
        }

        public static string OtherEndRole(this ObjectStateEntry relationshipEntry, EntityKey thisEndKey)
        {
            Debug.Assert(relationshipEntry != null);
            Debug.Assert(relationshipEntry.IsRelationship);
            Debug.Assert(thisEndKey != null);

            if ((EntityKey)relationshipEntry.UsableValues()[0] == thisEndKey)
            {
                return relationshipEntry.UsableValues().DataRecordInfo.FieldMetadata[1].FieldType.Name;
            }
            else if ((EntityKey)relationshipEntry.UsableValues()[1] == thisEndKey)
            {
                return relationshipEntry.UsableValues().DataRecordInfo.FieldMetadata[0].FieldType.Name;
            }
            else
            {
                throw new InvalidOperationException("Neither end of the relationship contains the passed in key.");
            }
        }

        public static bool IsEntityReference(this IRelatedEnd relatedEnd)
        {
            var relationshipType = relatedEnd.GetType();
            return (relationshipType.GetGenericTypeDefinition() == typeof(EntityReference<>));
        }

        public static EntityKey GetEntityKey(this IRelatedEnd relatedEnd)
        {
            Debug.Assert(relatedEnd.IsEntityReference());
            var relationshipType = relatedEnd.GetType();
            var pi = relationshipType.GetProperty("EntityKey");
            return (EntityKey)pi.GetValue(relatedEnd, null);
        }

        public static void SetEntityKey(this IRelatedEnd relatedEnd, EntityKey key)
        {
            Debug.Assert(relatedEnd.IsEntityReference());
            var relationshipType = relatedEnd.GetType();
            var pi = relationshipType.GetProperty("EntityKey");
            pi.SetValue(relatedEnd, key, null);
        }

        public static bool Contains(this IRelatedEnd relatedEnd, EntityKey key)
        {
            foreach (var relatedObject in relatedEnd)
            {
                Debug.Assert(relatedObject is IEntityWithKey);
                if (((IEntityWithKey)relatedObject).EntityKey == key)
                {
                    return true;
                }
            }
            return false;
        }


        ///////////////////////////////////////////////////////////////

        public static AssociationEndMember[] GetAssociationEnds(this ObjectStateEntry entry)
        {
            var fieldMetadata = entry.UsableValues().DataRecordInfo.FieldMetadata;

            return fieldMetadata.Select(m => m.FieldType as AssociationEndMember).ToArray();
        }

        public static AssociationEndMember GetOtherAssociationEnd(this ObjectStateEntry entry, AssociationEndMember end)
        {
            end.ValidateBelongsTo(entry);
            AssociationEndMember[] ends = entry.GetAssociationEnds();
            if (ends[0] == end)
            {
                return ends[1];
            }
            return ends[0];
        }

        public static EntityKey GetEndEntityKey(this ObjectStateEntry entry, AssociationEndMember end)
        {
            end.ValidateBelongsTo(entry);

            AssociationEndMember[] ends = entry.GetAssociationEnds();

            if (ends[0] == end)
            {
                return entry.UsableValues()[0] as EntityKey;
            }

            return entry.UsableValues()[1] as EntityKey;
        }

        public static NavigationProperty GetNavigationProperty(this ObjectStateEntry entry, AssociationEndMember end)
        {
            end.ValidateBelongsTo(entry);

            var otherEnd = entry.GetOtherAssociationEnd(end);
            var relationshipType = entry.EntitySet.ElementType;
            var key = entry.GetEndEntityKey(end);
            var entitySet = key.GetEntitySet(entry.ObjectStateManager.MetadataWorkspace);
            var property = entitySet.ElementType.NavigationProperties
                .SingleOrDefault(p => p.RelationshipType == relationshipType && p.FromEndMember == end && p.ToEndMember == otherEnd);
            return property;
        }

        static void ValidateBelongsTo(this AssociationEndMember end, ObjectStateEntry entry)
        {
            if (!entry.IsRelationship)
            {
                throw new ArgumentException("is not a relationship entry", "entry");
            }

            var fieldMetadata =
                    entry.UsableValues().DataRecordInfo.FieldMetadata;
            if (fieldMetadata[0].FieldType as AssociationEndMember != end &&
                fieldMetadata[1].FieldType as AssociationEndMember != end)
            {
                throw new InvalidOperationException(string.Format(
                        "association end {0} does not participate in the " +
                        "relationship {1}", end, entry));
            }
        }
    }
}