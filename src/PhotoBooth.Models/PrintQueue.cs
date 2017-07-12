using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoBooth.Models
{
    public class PrintQueue
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста, введите URL изображения")]
        [Display(Name = "Ссылка на фото")]
        public string BlobPathToImage { get; set; }
        [Required(ErrorMessage = "Пожалуйста, выберите рабочую фото будку")]
        [Display(Name = "Фото будка")]
        public Guid PhotoBoothEntityId { get; set; }
        public virtual PhotoBoothEntity PhotoBoothEntity { get; set; }
    }
}