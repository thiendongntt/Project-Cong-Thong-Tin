using CongThongTin_UTC2.CommonData;
using CongThongTin_UTC2.Models;
using CongThongTin_UTC2.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
namespace CongThongTin_UTC2.Controllers
{
    [Authorize(Roles = "admin,editor")]
    public class AdminController : Controller
    {
        UnitOfWork db = new UnitOfWork(new DBCongThongTin());
        //[Authorize(Roles ="admin,editor")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult newPost()
        {
            newPostViewModel model = new newPostViewModel
            {
                post_type = PostType.Normal,
                post_tag = PostData.getTagList(), //PostData.getTagList(),
                Status = true,
                changeAvatar = true,
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult newPost(newPostViewModel model)
        {
            List<Tag> taglist = new List<Tag>();
            taglist.AddRange(model.post_tag.Where(m => m.Selected)
                .Select(m => new Tag { TagID = int.Parse(m.Value), TagName = m.Text })
                );
           
            if (ModelState.IsValid && taglist.Count > 0)
            {
                // Nếu như teaser trống hoặc quá ngắn, sẽ dùng content thay thế từ content bài viết
                if (model.post_teaser == null || model.post_teaser.Length <= 20)
                {
                    model.post_teaser = CommonFunction.GetTeaserFromContent(model.post_content, 200);
                }
                //Upload ảnh và lưu ảnh với slug trùng tên tiêu đề bài viết
                bool isSavedSuccessfully = true;
                try
                {
                    if (model.avatarFile != null && model.avatarFile.ContentLength > 0)
                    {
                        string subPath = Server.MapPath("~/Upload/images/");
                        bool exists = System.IO.Directory.Exists(subPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subPath);
                        }
                        string extension = Path.GetExtension(model.avatarFile.FileName);
                        model.AvatarImage = SlugGenerator.SlugGenerator.GenerateSlug(model.post_title) +"-"+ new Random().Next(1,100) + extension;
                        model.avatarFile.SaveAs(Server.MapPath("~/Upload/images/") + model.AvatarImage);
                    }
                }
                catch (Exception ex)
                {
                    isSavedSuccessfully = false;
                }
                if (isSavedSuccessfully == true)
                {
                    User user = db.userRepository.FindByUsername(User.Identity.Name);
                    Post pOST = new Post
                    {
                        userid = user.userid,
                        create_date = DateTime.Now,
                        AvatarImage = model.AvatarImage,
                        post_content = model.post_content,
                        post_review = model.post_review,
                        post_title = model.post_title,
                        post_type = (int)model.post_type,
                        ViewCount = 0,
                        Rated = (int)model.Rated,
                        post_teaser = model.post_teaser,
                        status = model.Status,
                        post_tag = model.meta_tag,
                    };
                    foreach(var i in taglist)
                    {
                        Tag tags = db.tagRepository.FindByID(i.TagID);
                        pOST.Tags.Add(tags);
                        tags.Posts.Add(pOST);
                    }
                    string slug = SlugGenerator.SlugGenerator.GenerateSlug(pOST.post_title.ToLower());
                    pOST.post_slug = slug;
                    if(db.postRepository.AllPosts().Any(m=>m.post_slug == slug))
                    {
                        pOST.post_slug = slug + "-" + 1;
                    }
                    db.postRepository.AddPost(pOST);
                    db.Commit();
                    if (model.post_type.Equals(PostType.Slide))
                    {
                        return View("UploadImage", new uploadViewModel { id = pOST.post_id, title = pOST.post_title });
                    }
                    return RedirectToAction("MyPost", "Admin");
                }
            }
            // nếu lỗi thì đẩy về tiếp
            //newPostViewModel nmodel = new newPostViewModel
            //{
            //    post_type = PostType.Normal,
            //    post_tag = PostData.getTagList(),
            //};

            return View(model);
        }
        [Authorize(Roles = "admin")]
        public ActionResult ListPost(string sortOrder, string CurrentSort, int? page, string titleStr)
        {
            int pageSize = 100;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.CurrentSort = sortOrder;
            sortOrder = String.IsNullOrEmpty(sortOrder) ? "Title" : sortOrder;
            IPagedList<Post> post = null;
            ViewBag.Sort = "tăng dần";
            if (String.IsNullOrWhiteSpace(titleStr))
            {
                switch (sortOrder)
                {
                    case "Title":
                        ViewBag.sortname = "tiêu đề";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().OrderByDescending
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().OrderBy
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "CreateDate":
                        ViewBag.sortname = "ngày tạo";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().OrderByDescending
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().OrderBy
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        break;
                    case "ViewCount":
                        ViewBag.sortname = "lượt xem";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().OrderByDescending
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().OrderBy
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        break;
                   
                }
            }
            else
            {
                ViewBag.titleStr = titleStr;
                switch (sortOrder)
                {
                    case "Title":
                        ViewBag.sortname = "tiêu đề";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderByDescending
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderBy
                                      (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "CreateDate":
                        ViewBag.sortname = "ngày tạo";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderByDescending
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderBy
                                   (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "ViewCount":
                        ViewBag.sortname = "lượt xem";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderByDescending
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower())).OrderBy
                                     (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    
                }
            }
            return View(post);
        }
        public ActionResult MyPost(string sortOrder, string CurrentSort, int? page, string titleStr)
        {
     
            int pageSize = 100;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.CurrentSort = sortOrder;
            sortOrder = String.IsNullOrEmpty(sortOrder) ? "Title" : sortOrder;
            IPagedList<Post> post = null;
            ViewBag.Sort = "tăng dần";
            User user = db.userRepository.FindByUsername(User.Identity.Name);
            if (String.IsNullOrWhiteSpace(titleStr))
            {
                switch (sortOrder)
                {
                    case "Title":
                        ViewBag.sortname = "tiêu đề";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "CreateDate":
                        ViewBag.sortname = "ngày tạo";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        break;
                    case "ViewCount":
                        ViewBag.sortname = "lượt xem";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        break;
                       
                }
            }
            else
            {
                ViewBag.titleStr = titleStr;
                switch (sortOrder)
                {
                    case "Title":
                        ViewBag.sortname = "tiêu đề";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                    (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                      (m => m.post_title).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "CreateDate":
                        ViewBag.sortname = "ngày tạo";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                    (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                   (m => m.create_date).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                    case "ViewCount":
                        ViewBag.sortname = "lượt xem";
                        if (sortOrder.Equals(CurrentSort))
                        {
                            ViewBag.Sort = "giảm dần";
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                    (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }

                        else
                        {
                            post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                     (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                        }
                        break;
                       
                }
            }
            return View(post);
        }
        public ActionResult UserPost(int id, string sortOrder, string CurrentSort, int? page, string titleStr)
        {
          
            int pageSize = 100;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.CurrentSort = sortOrder;
            sortOrder = String.IsNullOrEmpty(sortOrder) ? "Title" : sortOrder;
            IPagedList<Post> post = null;
            ViewBag.Sort = "tăng dần";
            User user = db.userRepository.FindByID(id);
            if (user != null)
            {
                if (String.IsNullOrWhiteSpace(titleStr))
                {
                    switch (sortOrder)
                    {
                        case "Title":
                            ViewBag.sortname = "tiêu đề";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                        (m => m.post_title).ToPagedList(pageIndex, pageSize);
                            }

                            else
                            {
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                        (m => m.post_title).ToPagedList(pageIndex, pageSize);
                            }
                            break;
                        case "CreateDate":
                            ViewBag.sortname = "ngày tạo";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                        (m => m.create_date).ToPagedList(pageIndex, pageSize);
                            }

                            else
                            {
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                        (m => m.create_date).ToPagedList(pageIndex, pageSize);
                            }

                            break;
                        case "ViewCount":
                            ViewBag.sortname = "lượt xem";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderByDescending
                                        (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                            }

                            else
                            {
                                post = db.postRepository.AllPosts().Where(u => u.userid == user.userid).OrderBy
                                        (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                            }

                            break;
                         
                    }
                }
                else
                {
                    ViewBag.titleStr = titleStr;
                    switch (sortOrder)
                    {
                        case "Title":
                            ViewBag.sortname = "tiêu đề";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                        (m => m.post_title).ToPagedList(pageIndex, pageSize);
                            }
                            else
                            {
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                          (m => m.post_title).ToPagedList(pageIndex, pageSize);
                            }
                            break;
                        case "CreateDate":
                            ViewBag.sortname = "ngày tạo";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                        (m => m.create_date).ToPagedList(pageIndex, pageSize);
                            }

                            else
                            {
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                       (m => m.create_date).ToPagedList(pageIndex, pageSize);
                            }
                            break;
                        case "ViewCount":
                            ViewBag.sortname = "lượt xem";
                            if (sortOrder.Equals(CurrentSort))
                            {
                                ViewBag.Sort = "giảm dần";
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderByDescending
                                        (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                            }

                            else
                            {
                                post = db.postRepository.AllPosts().Where(m => m.post_title.ToLower().Contains(titleStr.ToLower()) && m.userid == user.userid).OrderBy
                                         (m => m.ViewCount).ToPagedList(pageIndex, pageSize);
                            }
                            break;
                           
                    }
                }
                return View(post);
            }
            else
            {
                return RedirectToAction("UserManager", "User");
            }


        }
        public ActionResult editPost(int id)
        {
            Post post = db.postRepository.FindByID(id);
            if (post == null)
            {
                return RedirectToAction("ListPost");
            }
            newPostViewModel model = new newPostViewModel();
            if (post.User.username == User.Identity.Name || User.IsInRole("admin"))
            {
                //Dynasty dn;
                //Enum.TryParse(post.dynasty, out dn);
                Rated rated;
                Enum.TryParse(post.Rated.ToString(), out rated);
                PostType type;
                Enum.TryParse(post.post_type.ToString(), out type);
                List<SelectListItem> tag = PostData.getTagList();
                //string[] oldtag = post.post_tag.Split(',');
                foreach (var x in tag)
                {
                    foreach (var t in post.Tags)
                    {
                        if (x.Value == t.TagID.ToString())
                        {
                            x.Selected = true;
                        }
                    }
                }
                model = new newPostViewModel
                {
                    post_id = post.post_id,
                    //dynasty = dn,
                    post_content = post.post_content,
                    AvatarImage = post.AvatarImage,
                    post_review = post.post_review,
                    changeAvatar = false,
                    imagepath = "/images/slides/" + post.post_id,
                    Rated = rated,
                    Status = post.status,
                    post_tag = tag,
                    post_teaser = post.post_teaser,
                    post_title = post.post_title,
                    post_type = type,
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("ListPost");
            }
        }
        [HttpPost]
        public ActionResult editPost(newPostViewModel model)
        {
            List<Tag> taglist = new List<Tag>();
            taglist.AddRange(model.post_tag.Where(m => m.Selected)
                .Select(m => new Tag { TagID = int.Parse(m.Value), TagName = m.Text })
                );
            List<Tag> untaglist = new List<Tag>();
            untaglist.AddRange(model.post_tag.Where(m => m.Selected == false)
                .Select(m => new Tag { TagID = int.Parse(m.Value), TagName = m.Text })
                );
            if (ModelState.IsValid && taglist.Count > 0)
            {
                // Nếu như teaser trống hoặc quá ngắn, sẽ dùng content thay thế từ content bài viết
                if (model.post_teaser == null || model.post_teaser.Length <= 20)
                {
                    model.post_teaser = CommonFunction.GetTeaserFromContent(model.post_content, 200);
                }
                //Upload ảnh và lưu ảnh với slug trùng tên tiêu đề bài viết
                bool isSavedSuccessfully = true;

                try
                {
                    if (model.changeAvatar == true && model.avatarFile != null && model.avatarFile.ContentLength > 0)
                    {
                        string subPath = Server.MapPath("~/Upload/images/");
                        bool exists = System.IO.Directory.Exists(subPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subPath);
                        }
                        //xóa ảnh cũ
                        bool exist = System.IO.File.Exists(Server.MapPath("~/Upload/images/" + model.AvatarImage));
                        if (exist)
                        {
                            System.IO.File.Delete(Server.MapPath("~/Upload/images/" + model.AvatarImage));
                        }
                        // lưu ảnh mới với tên là slug tiêu đề mới
                        string extension = Path.GetExtension(model.avatarFile.FileName);
                        model.AvatarImage = SlugGenerator.SlugGenerator.GenerateSlug(model.post_title) + "-" + model.post_id  + extension;
                        model.avatarFile.SaveAs(Server.MapPath("~/Upload/images/") + model.AvatarImage);
                    }
                }
                catch (Exception ex)
                {
                    isSavedSuccessfully = false;
                }
                if (isSavedSuccessfully)
                {
                    Post pOST = db.postRepository.FindByID(model.post_id);
                    if (pOST.User.username != User.Identity.Name)
                    {
                        return RedirectToAction("ListPost");
                    }
                    //pOST.dynasty = model.dynasty.ToString();
                    pOST.edit_date = DateTime.Now;
                    pOST.AvatarImage = model.AvatarImage;
                    pOST.post_content = model.post_content;
                    pOST.post_review = model.post_review;
                    pOST.post_title = model.post_title;
                    pOST.post_type = (int)model.post_type;
                    pOST.Rated = (int)model.Rated;
                    pOST.post_teaser = model.post_teaser;
                    pOST.status = model.Status;
                    pOST.post_tag = model.meta_tag;
                    foreach (var i in taglist)
                    {
                        Tag tags = db.tagRepository.FindByID(i.TagID);
                        if (!pOST.Tags.Contains(tags))
                        {
                            pOST.Tags.Add(tags);
                            tags.Posts.Add(pOST);
                        }
                        
                    }
                    foreach(var i in untaglist)
                    {
                        Tag tags = db.tagRepository.FindByID(i.TagID);
                        if (pOST.Tags.Contains(tags))
                        {
                            pOST.Tags.Remove(tags);
                            tags.Posts.Remove(pOST);
                        }
                    }
                    if(model.UpdateSlug)
                    {
                        string slug = SlugGenerator.SlugGenerator.GenerateSlug(model.post_title.ToLower());
                        pOST.post_slug = slug;
                        if (db.postRepository.AllPosts().Any(m => m.post_slug == slug))
                        {
                            pOST.post_slug = slug + "-" + 1;
                        }
                    }
                    db.postRepository.UpdatePost(pOST);
                    db.Commit();
                    if (model.post_type.Equals(PostType.Slide))
                    {
                        return View("UploadImage", new uploadViewModel { id = pOST.post_id, title = pOST.post_title });
                    }

                }

                return RedirectToAction("ListPost");

            }
            newPostViewModel nmodel = new newPostViewModel
            {
                post_type = PostType.Normal,
                post_tag = PostData.getTagList(),
            };

            return View(nmodel);
        }
        //POST: Tbl_POST/Delete/5
        [HttpPost]
        public JsonResult DeleteConfirmed(int id)
        {
            Post Tbl_POST = db.postRepository.FindByID(id);
            string title = Tbl_POST.post_title;
            int post_id = Tbl_POST.post_id;
            if ((Tbl_POST.User.username == User.Identity.Name) || User.IsInRole("admin"))
            {
                db.Context.Comments.RemoveRange(db.Context.Comments.Where(c => c.post_id == post_id));
                db.Commit();

                db.postRepository.DeletePost(Tbl_POST);
                if (Tbl_POST.post_type == (int)PostType.Slide)
                {
                    string subPath = Server.MapPath("~/Files/images/slides/" + Tbl_POST.post_id);
                    bool exists = System.IO.Directory.Exists(subPath);
                    if (exists)
                    {
                        System.IO.Directory.Delete(subPath, true);
                    }
                }
                // Xóa ảnh đại diện cũ
                bool exist = System.IO.File.Exists(Server.MapPath("~/Upload/images/" + Tbl_POST.AvatarImage));
                if (exist)
                {
                    System.IO.File.Delete(Server.MapPath("~/Upload/images/" + Tbl_POST.AvatarImage));
                }

                db.Commit();
                return Json(new { Message = "Xóa '" + title + "' thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = "Không thể xóa '" + title + "' <br /> vì đó không phải bài viết của bạn." }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult changeStatus(int id, bool state = false)
        {
            Post Tbl_POST = db.postRepository.FindByID(id);
            string title = Tbl_POST.post_title;
            Tbl_POST.status = state;
            string prefix = state == true ? "Đăng" : "Hủy đăng";
            db.Commit();
            return Json(new { Message = prefix + " \"" + title + "\" thành công" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListSeries(string name, int? page)
        {
            int pageSize = 100;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IPagedList<Series> sr = null;
            if (!String.IsNullOrWhiteSpace(name))
            {
                ViewBag.name = name;
                sr = db.seriesRepository.AllSeries().Where(m => m.seriesName.Contains(name)).OrderBy(x => x.seriesID).ToPagedList(pageIndex, pageSize);
            }
            else
            {
                sr = db.seriesRepository.AllSeries().OrderBy(x => x.seriesID).ToPagedList(pageIndex, pageSize);
            }
            return View(sr);
        }
        public ActionResult MyListSeries(string name, int? page)
        {
            
            User user = db.userRepository.FindByUsername(User.Identity.Name);
         
            int pageSize = 100;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IPagedList<Series> sr = null;
           
            if (!String.IsNullOrWhiteSpace(name))
            {
                ViewBag.name = name;
                sr = db.seriesRepository.AllSeries().Where(m => m.seriesName.Contains(name)).Where(t => t.userid == user.userid).OrderBy(x => x.seriesID).ToPagedList(pageIndex, pageSize);
            }
            else
            {
                sr = db.seriesRepository.AllSeries().Where(t => t.userid == user.userid).OrderBy(x => x.seriesID).ToPagedList(pageIndex, pageSize);
            }
            return View(sr);
        }
        public ActionResult SerieDetail(int id, string name)
        {
            Series sr = db.seriesRepository.FindByID(id);
            if (sr != null)
            {

                if (!String.IsNullOrWhiteSpace(name))
                {
                    using (DBCongThongTin conn = new DBCongThongTin())
                    {
                        var result = (
                                // instance from context
                            from a in conn.Series
                                    // instance from navigation property
                            from b in a.Posts
                                    //join to bring useful data
                            join c in conn.Posts on b.post_id equals c.post_id
                                //where a.seriesID == c.Tbl_Series.s
                            where a.seriesID.Equals(id)
                            where b.post_title.Contains(name)
                            select new ViewModel.SeriesPost
                                {
                                    post_id = b.post_id,
                                    post_title = b.post_title,
                                    status = b.status,
                                    ViewCount = b.ViewCount,
                                    userid = b.ViewCount,
                                    create_date = b.create_date,
                                userfullname = b.User.fullname,
                                username = b.User.username,
                                slug = b.post_slug
                            }).ToList();
                        SeriesPostViewModel postViewModel = new SeriesPostViewModel
                        {
                            SerieID = sr.seriesID,
                            SerieName = sr.seriesName,
                            ListPost = result,
                        };
                        return View(postViewModel);

                    }
                }
                else
                {
                    ViewBag.name = name;
                    using (DBCongThongTin conn = new DBCongThongTin())
                    {
                        var result = (
                            // instance from context
                            from a in conn.Series
                                // instance from navigation property
                            from b in a.Posts
                                //join to bring useful data
                            join c in conn.Posts on b.post_id equals c.post_id
                            //where a.seriesID == c.Tbl_Series.s
                            where a.seriesID.Equals(id)
                            select new ViewModel.SeriesPost
                            {
                                post_id = b.post_id,
                                post_title = b.post_title,
                                status = b.status,
                                ViewCount = b.ViewCount,
                                userid = b.ViewCount,
                                create_date = b.create_date,
                                userfullname = b.User.fullname,
                                username = b.User.username,

                            }).ToList();
                        SeriesPostViewModel postViewModel = new SeriesPostViewModel
                        {
                            SerieID = sr.seriesID,
                            SerieName = sr.seriesName,
                            ListPost = result,
                        };
                        return View(postViewModel);

                    }
                }
                //return View(postlist);
            }
            return RedirectToAction("ListSeries");

        } 
        /// <summary>
        /// Thêm series mới
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult addSerie(string name)
        {
            User user = db.userRepository.FindByUsername(User.Identity.Name);
            if (String.IsNullOrWhiteSpace(name))
            {
                Response.StatusCode = 500;
                return Json(new { Message = "Không được để trống tiêu đề series" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                db.seriesRepository.AddSeries(new Series
                {
                    seriesName = name,
                    userid = user.userid,
                });
                db.Commit();
                return Json(new { Message = "Tạo series" + " \"" + name + "\" thành công" }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult editSerie(int id,string name)
        {
            if (string.IsNullOrWhiteSpace(name) || id == null)
            {
                Response.StatusCode = 500;
                return Json(new { Message = "Không được để trống tiêu đề series hoặc id" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Series series = db.seriesRepository.FindByID(id);
                series.seriesName = name;
                db.Commit();
                return Json(new { Message = "Edit series" + " \"" + series.seriesName + "\" thành công" }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult DeleteSeries(int id)
        {
            Series cate = db.seriesRepository.FindByID(id);
            string title = cate.seriesName;
            db.seriesRepository.Delete(cate);
            db.Commit();
            return Json(new { reload = true, Message = "Xóa '" + title + "' thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddToSerie(int? id,int? seriid)
        {
            if (id == null || seriid == null)
            {
                Response.StatusCode = 500;
                return Json(new { Message = "Lỗi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                using (DBCongThongTin conn = new DBCongThongTin())
                {
                    Series sr = conn.Series.Find(seriid);
                    Post post = conn.Posts.Find(id);
                    foreach(var x in sr.Posts)
                    {
                        if(x.post_id.Equals(id))
                        {
                            Response.StatusCode = 500;
                            return Json(new { Message = "Trùng lặp bài viết" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    sr.Posts.Add(post);
                    post.Series.Add(sr);
                    conn.SaveChanges();
                }
                    return Json(new { Message = "Thêm thành công" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult checkPost(int id)
        {
            Post p = db.postRepository.FindByID(id);
            if (p != null)
            {
                return Json(new { valid = true, Message = p.post_title }, JsonRequestBehavior.AllowGet);
            }
            //Response.StatusCode = 500;
            return Json(new { valid = false, Message = "ID nhập không hợp lệ" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveFromSerie(int? id,int? seriid)
        {
            if (id == null || seriid == null)
            {
                Response.StatusCode = 500;
                return Json(new { Message = "Lỗi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                using (DBCongThongTin conn = new DBCongThongTin())
                {
                    Series sr = conn.Series.Find(seriid);
                    Post post = conn.Posts.Find(id);
                    sr.Posts.Remove(post);
                    post.Series.Remove(sr);
                    conn.SaveChanges();
                }
                return Json(new { Message = "Remove thành công" }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Trình quản lý file
        /// </summary>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        [HttpGet]
       public ActionResult FileManager(string subFolder)
        {
            FileViewModel model = new FileViewModel { Folder = "Files", SubFolder = subFolder };
            return View(model);
        }
        
        [HttpPost]
        public ActionResult UploadImage(uploadViewModel model)
        {
            if(ModelState.IsValid)
            {
                bool isSavedSuccessfully = true;
                try
                {
                    /////throw new Exception();
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[fileName];
                        //Save file content goes here
                        if (file != null && file.ContentLength > 0)
                        {
                            string subPath = Server.MapPath("~/Upload/Files/Images/slides/" + model.id);
                            bool exists = System.IO.Directory.Exists(subPath);
                            if (!exists)
                            {
                                System.IO.Directory.CreateDirectory(subPath);
                            }
                            file.SaveAs(Server.MapPath("~/Upload/Files/Images/slides/" + model.id + "/") + file.FileName);
                            //return Json(new { Message = "Tải file "+file.FileName+" lên thành công." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    isSavedSuccessfully = false;
                }
                if (isSavedSuccessfully == false)
                {
                    Response.StatusCode = 500;
                    return Json(new { Message = "Tải file lên bị lỗi." }, JsonRequestBehavior.AllowGet);
                    //return Json(new {success= false, Message = "Error in saving file" });
                }
                else
                {
                    Response.StatusCode = 200;
                    return Json(new { Message = "Hoàn tất upload ảnh.", Url = "/Admin/ListPost" }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("ListPost");
                }
                   
            }
            return View();
        }
        [Authorize(Roles ="admin")]
        public ActionResult InfoChange()
        {
            WebInfo info = db.infoRepository.FindByID();
            infoViewModel model = new infoViewModel
            {
                web_name = info.web_name,
                web_about = info.web_about,
                web_des = info.web_des
            };
            return View(model);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult InfoChange(infoViewModel model)
        {
            if(ModelState.IsValid)
            {
                WebInfo info = db.infoRepository.FindByID();
                info.web_des = model.web_des;
                info.web_name = model.web_name;
                info.web_about = model.web_about;
                db.Commit();
            }
            return View();
        }
        //[Authorize(Roles = "admin")]
        public ActionResult ListTags()
        {
            List<ListTagViewModel> listTags = db.tagRepository.AllTags()
                .Select(m => new ListTagViewModel
                {
                    TagID = m.TagID,
                    TagName = m.TagName,
                    PostCount = m.Posts.Count,
                }).ToList();
            return View(listTags);
        }
        public ActionResult MyListTags()
        {
            User user = db.userRepository.FindByUsername(User.Identity.Name);
            List<ListTagViewModel> listTags = db.tagRepository.AllTags()
                .Where(t => t.userid == user.userid)
                .Select(m => new ListTagViewModel

                {
                    TagID = m.TagID,
                    TagName = m.TagName,
                    PostCount = m.Posts.Count,
                }).ToList();
            return View(listTags);
        }
        [HttpPost]
        public JsonResult DeleteTag(int id)
        {
            Tag tags = db.tagRepository.FindByID(id);
            if(tags.Posts.Count > 0)
            {
                Response.StatusCode = 500;
                return Json(new { reload = false, Message = "Tags còn chứa bài viết. Không xóa được!" }, JsonRequestBehavior.AllowGet);
            }
            db.tagRepository.DeleteTag(tags);
            db.tagRepository.SaveChanges();
            return Json(new { reload = true, Message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateTag(int id,string name)
        {
            Tag tags = db.tagRepository.FindByID(id);
            if(String.IsNullOrWhiteSpace(name))
            {
                Response.StatusCode = 500;
                return Json(new { reload = true, Message = "Chưa nhập tên" }, JsonRequestBehavior.AllowGet);
            }
            tags.TagName = name;
            db.tagRepository.SaveChanges();
            return Json(new { reload = true, Message = "Sửa '"+tags.TagName+"' thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult NewTag(string name)
        {
            User user = db.userRepository.FindByUsername(User.Identity.Name);
            if (String.IsNullOrWhiteSpace(name))
            {
                Response.StatusCode = 500;
                return Json(new { reload = true, Message = "Chưa nhập tên" }, JsonRequestBehavior.AllowGet);
            }
            Tag tags = new Tag
            {
                TagName = name,
                userid = user.userid
            };
            db.tagRepository.AddTag(tags);
            db.tagRepository.SaveChanges();
            return Json(new { reload = true, Message = "Thêm '" + tags.TagName + "' thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MyNewTag(string name)
        {
            User user = db.userRepository.FindByUsername(User.Identity.Name);

            if (String.IsNullOrWhiteSpace(name))
            {
                Response.StatusCode = 500;
                return Json(new { reload = true, Message = "Chưa nhập tên" }, JsonRequestBehavior.AllowGet);
            }
            Tag tags = new Tag
            {
                TagName = name,
                userid = user.userid
            };
            db.tagRepository.AddTag(tags);
            db.tagRepository.SaveChanges();
            return Json(new { reload = true, Message = "Thêm '" + tags.TagName + "' thành công" }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "admin")]
        public ViewResult HotPostManager()
        {
            List<StickyPost> posts = db.hotPostRepository.AllPosts().ToList();
            return View(posts);
        }
        [HttpPost]
        public JsonResult editHotPost(int id, int priority)
        {
            StickyPost hotPost = db.hotPostRepository.FindByID(id);
            hotPost.priority = priority;
            db.hotPostRepository.SaveChanges();
            return Json(new { reload = true, Message = "Sửa '" + hotPost.Post.post_title + "' thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult deleteHotPost(int id)
        {
            
            StickyPost hotPost =  db.hotPostRepository.FindByID(id);
            string tit = hotPost.Post.post_title;
            db.hotPostRepository.DeletePost(hotPost);
            db.hotPostRepository.SaveChanges();
            return Json(new { reload = true, Message = "Gỡ bài ghim '" + tit + "' thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult addhotPost(int id, int priority)
        {
            StickyPost hotPost = new StickyPost
            {
                post_id = id,
                priority = priority,
            };
            db.hotPostRepository.AddHotPost(hotPost);
            db.hotPostRepository.SaveChanges();
            return Json(new { reload = true, Message = "Ghim bài thành công" }, JsonRequestBehavior.AllowGet);
        }

        // Lấy dữ liệu từ datase của tất cả các ngày (tên bài viết, số lượt xem, số lượt bình luận) để truyền giá trị vào đồ thị
        public ActionResult GetData()
        {
            var nowDate = DateTime.Now;
            var queryNew = (from post in db.Context.Posts
                            let convertDate = (DateTime)post.create_date
                            join com in db.Context.Comments
                            on post.post_id equals com.post_id
                            let DateComment = (DateTime)com.CommentDate
                            where convertDate.Day >= nowDate.Day && convertDate.Month >= nowDate.Month && convertDate.Year >= nowDate.Year
                           && DateComment.Day >= nowDate.Day && DateComment.Month >= nowDate.Month && DateComment.Year >= nowDate.Year
                            group post by new
                            {
                                post.post_title,
                                post.ViewCount
                            } into g

                            select new
                            {
                                name = g.Key.post_title,
                                count = g.Key.ViewCount,
                                countComment = g.Count()
                            }).ToList();

            return Json(queryNew, JsonRequestBehavior.AllowGet);
        }
        // Lấy dữ liệu từ database của ngày hiện tại, truyền giá trị vào đồ thị
        public ActionResult GetDataNow()
        {
           
            var query = (from post in db.Context.Posts
                     
                         join com in db.Context.Comments
                         on post.post_id equals com.post_id
                         group post by new
                         {
                             post.post_title,
                             post.ViewCount
                         } into g

                         select new
                         {
                             name = g.Key.post_title,
                             count = g.Key.ViewCount,
                             countComment = g.Count()
                         }).ToList();

            return Json(query, JsonRequestBehavior.AllowGet);
        }

    }
}