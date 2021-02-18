using System;

namespace Sample.MultilingualContent.Entities
{
    public abstract class EntityBase
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Record was deleted. 
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// When record created at. (UTC)
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// When record updated at. (UTC)
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// When record deleted at. (UTC)
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; } = null;
    }
}
