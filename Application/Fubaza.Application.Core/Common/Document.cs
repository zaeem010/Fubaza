using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fubaza.Application.Core.Common
{
    public abstract class Document 
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(256)]
        public string? FileName { get; set; }

        [MaxLength(8)]
        public string? Extension { get; set; }

        public bool IsDeleted { get; set; }

        [MaxLength(512)]
        public string? FileUrl { get; set; }

        [NotMapped]
        public string? Base64 { get; set; }

        [NotMapped]
        public byte[]? FileContent { get; set; }  // Nullable byte[] to avoid initialization issues

        [NotMapped]
        public string? FileDirectoryPath { get; set; }  // Converted to property

        [NotMapped]
        public int FileId { get; set; }
    }

}
