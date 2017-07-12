using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PhotoBooth.Models
{
    public class Photo
    {
        [Key]
        public Guid Id { get; set; }
        public string LocalPathToImage { get; set; }
        [Display(Name = "Путь")]
        public string BlobPathToImage { get; set; }
        [Display(Name = "Путь превью")]
        public string BlobPathToPreviewImage { get; set; }
        [Display(Name = "MD5")]
        public string Md5Hash { get; set; }
        public Guid PhotoEventId { get; set; }
        public virtual PhotoEvent PhotoEvent { get; set; }
        public Guid? OriginalPhotoEventId { get; set; }
        [Display(Name = "Длина")]
        public int ImageWidth { get; set; }
        [Display(Name = "Высота")]
        public int ImageHeight { get; set; }
        [Display(Name = "Не показывать")]
        public bool IsDeleted { get; set; }
        [Display(Name = "Время создания")]
        public DateTime Created { get; set; }

        [NotMapped]
        public string ImageName 
        {
            get { return Path.GetFileName(BlobPathToImage); }
        }
        [NotMapped]
        public string PreviewImageName 
        {
            get { return Path.GetFileName(BlobPathToPreviewImage); }
        }
        [NotMapped]
        public Guid PhotoEventUuid 
        {
            get
            {
                return (OriginalPhotoEventId.HasValue && OriginalPhotoEventId.Value != Guid.Empty) ? OriginalPhotoEventId.Value : PhotoEventId;
            }
        }
    }
}