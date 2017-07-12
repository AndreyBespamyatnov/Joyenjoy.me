using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PhotoBooth.Models
{
    public class InstagrammBrandingValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as HttpPostedFileBase;
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            using (Image image = Image.FromStream(file.InputStream))
            {
                if (!image.RawFormat.Equals(ImageFormat.Png))
                {
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                }

                if (image.Size.Width != 640 || image.Size.Height != 320)
                {
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                }
            }

            return null;
        }
    }

    public class TwitterHashtagAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return null;
            }

            var inputString = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return null;
            }

            int hashCount = inputString.Count(c => c == '#');
            bool hasWhiteSpaces = inputString.Any(char.IsWhiteSpace);
            
            if (inputString.Trim() != "#" && inputString.Substring(0, 1) == "#" && hashCount == 1 && !hasWhiteSpaces && inputString.Substring(1).All(char.IsLetterOrDigit))
            {
                return null;
            }

            return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
        }
    }

    public class PhotoEvent
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Название")]
        [Required(ErrorMessage = "Пожалуйста, введите название события")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "#Хештег")]
        [TwitterHashtag(ErrorMessage = "Пожалуйста, введите верный хешрег в формате '#имяхештега'")]
        public string HashTag { get; set; }

        [Display(Name = "Начало")]
        [Required(ErrorMessage = "Пожалуйста, введите дату начала события")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDateTime { get; set; }

        [Display(Name = "Окончание")]
        [Required(ErrorMessage = "Пожалуйста, введите дату окончания события")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDateTime { get; set; }

        [Display(Name = "Показывать в галерее")]
        public bool ShowOnGallery { get; set; }

        [Display(Name = "Запрашивать пароль")]
        public bool IsPublic { get; set; }

        [Display(Name = "Пароль на вход в галерею")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Архив Фото")]
        public string LinkToLastZip { get; set; }

        [Display(Name = "Превью галереи")]
        public string LinkToGalleryPreviewImage { get; set; }

        [Display(Name = "Создано")]
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        [Display(Name = "Брендинг для Инстаграмма")]
        public string InstagrammBrandingImage { get; set; }
         
        [NotMapped]
        [InstagrammBrandingValidator(ErrorMessage = "Пожалуйста, загрузите изображение в формате '.png', с размером '640x320'")]
        public HttpPostedFileBase InstagrammBrandingFile { get; set; }

        [NotMapped]
        public DateTime CreatedLocal
        {
            get
            {
                return Created.AddHours(3);
            }
        }

        public virtual ICollection<Photo> Photos { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите рабочую фото будку")]
        [Display(Name = "Фото будка")]
        public Guid PhotoBoothEntityId { get; set; }
        public virtual PhotoBoothEntity PhotoBoothEntity { get; set; }

        [NotMapped]
        public string FirstPhoto
        {
            get
            {
                var photo = this.Photos.FirstOrDefault(p => !p.IsDeleted);

                if (photo == null)
                {
                    return null;
                }

                return "/EventImages/" + this.Id + "/" + photo.PreviewImageName;
            }
        }
    }
}