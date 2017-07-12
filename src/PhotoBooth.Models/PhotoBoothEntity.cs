using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PhotoBooth.Models
{
    public class PhotoBoothEntity
    {
        [DisplayName("Уникальный номер Фото Будки")]
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Пожалуйста, введите название фото будки")]
        public string Name { get; set; }
        [Display(Name = "Корневая папка с фото")]
        [Required(ErrorMessage = "Пожалуйста, задайте корневую папку с фото")]
        public string PathToDSLRSettings { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите UUID номер айпада")]
        [Display(Name = "IPad UUID")]
        public string IPadDeviceId { get; set; }

        [Display(Name = "Последняя активность")]
        public DateTime? LastAvailableDate { get; set; }
        public virtual ICollection<PhotoEvent> Events { get; set; }
    }
}
