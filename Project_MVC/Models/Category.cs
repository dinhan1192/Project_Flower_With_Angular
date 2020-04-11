﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_MVC.Models
{
    public class Category
    {
        [Key]
        [DisplayName("Category Code")]
        //[RegularExpression(@"^[A-Z]{1,3}\d{1,4}$", ErrorMessage = "Invalid Code")]
        [RegularExpression(@"^[A-Z]+$", ErrorMessage = "Invalid Code, Code only contain capital characters")]
        public string Code { get; set; }
        [DisplayName("Category Name")]
        [Required]
        [RegularExpression(@"^[0-9a-zA-Z\s+ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềếểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ]+$", ErrorMessage = "Invalid Product Category Name")]
        public string Name { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        [DisplayName("Image")]
        public string ImageUrl { get; set; }
        [DisplayName("Created At")]
        public DateTime? CreatedAt { get; set; }
        [DisplayName("Updated At")]
        public DateTime? UpdatedAt { get; set; }
        [DisplayName("DeletedBy At")]
        public DateTime? DeletedAt { get; set; }
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Updated By")]
        public string UpdatedBy { get; set; }
        [DisplayName("Deleted By")]
        public string DeletedBy { get; set; }
        public CategoryStatus Status { get; set; }
        public virtual ICollection<Flower> Flowers { get; set; }
        //[ForeignKey("LevelOneProductCategory")]
        [DisplayName("Parent Code")]
        public string ParentCode { get; set; }
        //public virtual LevelOneProductCategory LevelOneProductCategory { get; set; }
        [DisplayName("Parent Code")]
        [NotMapped]
        [RegularExpression(@"^[0-9A-Z]+\s-\s[0-9a-zA-Z\s+ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềếểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ]+$", ErrorMessage = "Invalid Product Category")]
        public string ParentNameAndCode { get; set; }

        public enum CategoryStatus
        {
            NotDeleted = 0, Deleted = -1
        }

        internal bool IsDeleted()
        {
            return this.Status == CategoryStatus.Deleted;
        }
    }
}