﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Project_MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_MVC.Services
{
    public class MySQLImageService : IImageService
    {
        private MyDbContext _db;
        public MyDbContext DbContext
        {
            get { return _db ?? HttpContext.Current.GetOwinContext().Get<MyDbContext>(); }
            set { _db = value; }
        }
        private IUserService userService;
        private IRatingFlowerService ratingFlowerService;

        public MySQLImageService()
        {
            userService = new UserService();
            ratingFlowerService = new RatingFlowerService();
        }

        public void DeleteImage(FlowerImage flowerImage)
        {
            DbContext.FlowerImages.Remove(flowerImage);
            DbContext.SaveChanges();
        }

        public FlowerImage DetailImage(int? id)
        {
            return DbContext.FlowerImages.Find(id);
        }

        public List<FlowerImage> GetList()
        {
            return DbContext.FlowerImages.ToList();
        }

        public bool Rating(decimal rating, string code)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var ratingFlowersList = DbContext.RatingFlowers.Where(s => s.FlowerCode == code
            && s.UserId == userId).ToList();
            var type = "";
            decimal oldRating = 0;

            if (ratingFlowersList.Count == 0 || ratingFlowersList == null)
            {
                ratingFlowerService.CreateRating(rating, code, HttpContext.Current.User.Identity.GetUserId());
                type = Constant.CreateRating;
            }
            else
            {
                oldRating = ratingFlowerService.UpdateRating(rating, ratingFlowersList.FirstOrDefault().Id);
                type = Constant.UpdateRating;
            }

            ratingFlowerService.UpdateFlowerRating(rating, code, type, oldRating);
            //existProduct.Rating = rating;

            return true;
        }

        //public bool Rating(decimal rating, int? lectureId)
        //{
        //    var userId = HttpContext.Current.User.Identity.GetUserId();
        //    var customerLectureInteractList = DbContext.CustomerLectureInteracts.Where(s => s.LectureId == lectureId
        //    && s.UserId == userId).ToList();
        //    var type = "";

        //    if(customerLectureInteractList.Count == 0 || customerLectureInteractList == null)
        //    {
        //        customerLectureInteractService.CreateRating(rating, lectureId, HttpContext.Current.User.Identity.GetUserId());
        //        type = Constant.CreateRating;
        //    }
        //    else
        //    {
        //        customerLectureInteractService.UpdateRating(rating, customerLectureInteractList.FirstOrDefault().Id);
        //        type = Constant.UpdateRating;
        //    }

        //    var productCode = customerLectureInteractService.UpdateLectureRating(rating, lectureId, type);

        //    customerLectureInteractService.UpdateProductRating(productCode);
        //    //existProduct.Rating = rating;

        //    return false;
        //}

        public List<FlowerImage> SaveImage2List(string code, int? type, string strImageUrl)
        {
            var imageList = new List<FlowerImage>();
            if (!string.IsNullOrEmpty(strImageUrl))
            {
                string newStrlImageUrl = strImageUrl.Substring(1, strImageUrl.Length - 1);
                string[] imageUrlList = newStrlImageUrl.Split(new char[] { ',' });
                foreach (var item in imageUrlList)
                {
                    imageList.Add(new FlowerImage()
                    {
                        FlowerCode = code,
                        ImageUrl = item,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userService.GetCurrentUserName()
                    });
                }
            }

            return imageList;
        }

        //public List<LectureVideo> SaveVideo2List(int? id, IEnumerable<HttpPostedFileBase> videos, ModelStateDictionary state)
        //{
        //    if (videos != null)
        //    {
        //        var videoList = new List<LectureVideo>();
        //        foreach (var video in videos)
        //        {
        //            if (video != null)
        //            {
        //                using (var br = new BinaryReader(video.InputStream))
        //                {
        //                    if(ValidateVideo(video.FileName, state))
        //                    {
        //                        var data = br.ReadBytes(video.ContentLength);
        //                        var contentType = video.ContentType;
        //                        var vid = new LectureVideo { LectureId = id };
        //                        vid.Name = video.FileName;
        //                        if (char.IsDigit(vid.Name[1]))
        //                        {
        //                            vid.DisplayOrder = Convert.ToInt32(vid.Name.Substring(0, 2));
        //                        }
        //                        else
        //                        {
        //                            vid.DisplayOrder = Convert.ToInt32(vid.Name[0].ToString());
        //                        }
        //                        ValidateVideoDisplayOrder(vid.DisplayOrder, (int)id, state);
        //                        vid.VideoData = data;
        //                        vid.ContentType = contentType;
        //                        vid.CreatedAt = DateTime.Now;
        //                        vid.CreatedBy = userService.GetCurrentUserName();
        //                        videoList.Add(vid);
        //                    }
        //                }
        //            }
        //        }
        //        return videoList;
        //    }

        //    return null;
        //}

        public bool ValidateVideo(string videoName, ModelStateDictionary state)
        {
            if (!char.IsDigit(videoName[0]))
            {
                state.AddModelError("LectureVideoValidation", "Tên file của Video bài giảng phải bắt đầu bằng số.");
                return false;
            }

            return true;
        }

        public void ValidateVideoDisplayOrder(int displayOrder, int parentId, ModelStateDictionary state)
        {
            throw new NotImplementedException();
        }

        //public void ValidateVideoDisplayOrder(int displayOrder, int parentId, ModelStateDictionary state)
        //{
        //    var list = DbContext.LectureVideos.Where(s => s.DisplayOrder == displayOrder && s.LectureId == parentId).ToList();
        //    if (list.Count != 0)
        //    {
        //        state.AddModelError("LectureVideoValidation", "Có video trùng thứ tự sắp xếp.");
        //    }

        //    if(displayOrder < Constant.FirstDisplayOrder)
        //    {
        //        state.AddModelError("LectureVideoValidation", "Số thứ tự không thể nhỏ hơn 1");
        //    }
        //}
    }
}