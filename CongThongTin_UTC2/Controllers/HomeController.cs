using CongThongTin_UTC2.Models;
using CongThongTin_UTC2.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Comment = CongThongTin_UTC2.Models.Comment;

namespace CongThongTin_UTC2.Controllers
{
    public class HomeController : Controller
    {
        UnitOfWork db = new UnitOfWork(new DBCongThongTin());

        // Lấy ra tất cả dữ liệu của 7 bài viết với điều kiện: status = true và sắp xếp theo thứ tự giảm dần (Xuất hiện ở trang chủ)
        public ActionResult Index()
        {
            ViewBag.Title = db.infoRepository.FindByID(1).web_name;
            return View(db.postRepository.AllPosts().Where(m=>m.status==true)
                .OrderByDescending(m=>m.create_date).Take(7).ToList());
        }

      
        // Ghim thứ hạng của 3 bài ở trang chủ
        public ViewResult _HotPost()
        {
            return View();
        }
        

       

        // Khi người dùng click vào 1 bài viết, thì ta sẽ có được tên của bài viết, ta sẽ gán lại tên bài viết cho biến title
        // Nếu như tên bài viết có tồn tại trong dữ liệu, thì xuất ra giao diện
        // Lấy ra các bình luận đã được người dùng bình luận trong bài viết và các thẻ tag của bài viết

        public ActionResult ViewPost(string title)
        {
            List<CommentList> commentList = new List<CommentList>();
            if (!String.IsNullOrWhiteSpace(title))
            {
                Post p = db.postRepository.FindBySlug(title);

                Models.Comment c = db.commentRepository.AllComments()
                                   .Where(m => m.post_id == p.post_id)
                                   .FirstOrDefault();
                if (c != null)
                {

                commentList  = c.Post.Comments.Select( m => new CommentList
                { 
                  CommentText = m.CommentText,
                  CommentID = m.CommentID,
                  CommentDate = m.CommentDate,
                  userid = m.userid,                 
                  post_id = m.post_id
                }).ToList();
                
                }
      
                if (p != null)
                {
                    p.ViewCount++;
                    db.Commit();
                    List<TagList> tagLists = p.Tags.Select(m => new TagList
                    {
                        id = m.TagID,
                        name = m.TagName,
                        slug = SlugGenerator.SlugGenerator.GenerateSlug(m.TagName) + "-" + m.TagID
                    }).ToList();
                if(c != null)
                    {

                    }
                  
                    return View(new ViewPostViewModel
                    {
                        post_id = p.post_id,
                        create_date = p.create_date,
                        post_review = p.post_review,
                        post_tag = p.post_tag,
                        AvatarImage = p.AvatarImage,
                        edit_date = p.edit_date,
                        post_content = p.post_content,
                        post_teaser = p.post_teaser,
                        post_title = p.post_title,
                        post_type = p.post_type,
                        Rated = p.Rated,
                        userid = p.userid,
                        ViewCount = p.ViewCount,
                        tagLists = tagLists,
                        commentList = commentList
                    });

                    
                }
                return RedirectToAction("Index");
            }
            return HttpNotFound();
          
        }
        // Thêm bình luận dưới mỗi bài viết

        [HttpPost]
        public ActionResult CommentController(int post_id, string CommentText, string post_slug)
        {
            // Nếu chưa đăng nhập thì cần phải đăng nhập
            var st = Session["UserDangNhap"];
            if (Convert.ToInt32(st) == 0)
            {
                return RedirectToAction("DangNhapUser", "User");
            }
            // Lấy id của người đăng nhập hiện tại
            int userid = Convert.ToInt32(Session["TaiKhoan"].ToString());

            // Nếu id đã có trong bài viết thì thêm 1 bình luận mới nữa (ko bỏ đi bình luận cũ), nếu chưa có thì thêm vào bình luận
            Comment comment = new Comment();
            if(CommentText != "")
            {
            var userComment = db.commentRepository.AllComments()
                              .Where(m => m.post_id == post_id)
                              .Where(m => m.userid == userid)
                              .Select(s => new Comment
                              {
                                  userid = userid,
                                  post_id = post_id,
                                  CommentText = CommentText,
                                  CommentDate = DateTime.Now,
                                  status = 1,
                              });
            if(userComment == null)
            {
                db.commentRepository.AddComment((Comment)userComment);
                db.Commit();
            }
            else
            {
              comment = new Comment { 
              CommentDate = DateTime.Now,
              CommentText = CommentText,
              userid = userid,
              post_id = post_id,
              status = 1,          
            };

            }
  
            db.commentRepository.AddComment(comment);
            db.Commit();
            }
            var id_comment = db.commentRepository.FindByID(comment.CommentID);
            Session["RepComment"] = post_slug;

            return RedirectToRoute("Baiviet", new { 
            controller = "Home", action = "ViewPost", title = post_slug
            });
        }
        [HttpPost]
        public ActionResult RepCommentController(int post_id, string RepCommentText, string post_slug)
        {
            var st = Session["UserDangNhap"];
            if (Convert.ToInt32(st) == 0)
            {
                return RedirectToAction("DangNhapUser", "User");
            }
            Comment cm = (Comment)Session["RepComment"];
            int userid = Convert.ToInt32(Session["TaiKhoan"].ToString());
            Comment comment = new Comment();
            var userComment = db.commentRepository.AllComments()
                              .Where(m => m.CommentID == cm.CommentID)
                              .Where(m => m.post_id == post_id)
                              .Where(m => m.userid == userid)                          
                              .Select(s => new Comment
                              {
                                  userid = userid,
                                  post_id = post_id,
                                  CommentText = RepCommentText,
                                  CommentDate = DateTime.Now,
                                  status = 1,
                              });
            if (userComment == null)
            {
                db.commentRepository.AddComment((Comment)userComment);
                db.Commit();
            }
            else
            {
                comment = new Comment
                {
                    CommentDate = DateTime.Now,
                    CommentText = RepCommentText,
                    userid = userid,
                    post_id = post_id,
                    status = 1,
                };

            }

            db.commentRepository.AddComment(comment);
            db.Commit();

            return RedirectToRoute("Baiviet", new
            {
                controller = "Home",
                action = "ViewPost",
                title = post_slug
            });

            return View();
        }

        // Xóa comment với điều kiện chỉ được xóa bình luận của người đó

        [HttpPost]
        public ActionResult DeleteComment(string post_slug, int CommentID)
        {
            var st = Session["UserDangNhap"];
            if (Convert.ToInt32(st) == 0)
            {
                return RedirectToAction("DangNhapUser", "User");
            }
            int userid = Convert.ToInt32(Session["TaiKhoan"].ToString());

            var commentUserid = db.commentRepository.FindByID(CommentID);

            //Nếu như id của người đăng nhập trùng với id của người bình luận thì mới cho xóa
            if(userid == commentUserid.userid)
            {
            db.commentRepository.DeleteComment(commentUserid);
            db.Commit();

            }

            return RedirectToRoute("Baiviet", new
            {
                controller = "Home",
                action = "ViewPost",
                title = post_slug
            });
        }

        public ActionResult Category(int? id, int? page)
        {
            if(id!=null)
            {
                
                int pageSize = 15;
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                Tag tag = db.tagRepository.FindByID(id.Value);
                if (tag != null)
                {
                    using (DBCongThongTin conn = db.Context)
                    {
                        var result = (
                            // instance from context
                            from a in conn.Tags
                                // instance from navigation property
                        from b in a.Posts
                            //join to bring useful data
                        join c in conn.Posts on b.post_id equals c.post_id
                            where a.TagID == id && b.status == true
                            orderby b.create_date descending
                            select new lstPostViewModel
                            {
                                post_id = c.post_id,
                                post_title = c.post_title,
                                post_teaser = c.post_teaser,
                                ViewCount = c.ViewCount,
                                AvatarImage = c.AvatarImage,
                                create_date = c.create_date,
                                slug = c.post_slug
                            }).ToPagedList(pageIndex, pageSize);
                        ViewBag.catname = tag.TagName;
                        return View(result);
                    }
                }
                return HttpNotFound();
            }

            return View("CategoryAll");

        }

        // Chức năng tìm kiếm theo tên bài viết hoặc là thẻ tag (tìm kiếm nâng cao )
        public ActionResult Search(SearchViewModel model,int? page)
        {
            int pageSize = 8;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IPagedList<lstPostViewModel> post = new List<lstPostViewModel>().ToPagedList(pageIndex, pageSize);
            List<Tag> taglist = new List<Tag>();
            taglist.AddRange(model.post_tag.Where(m => m.Selected)
                .Select(m => new Tag { TagID = int.Parse(m.Value), TagName = m.Text })
                );
            ViewBag.stitle = model.title;
            bool title = String.IsNullOrWhiteSpace(model.title);
            bool tag = taglist.Count == 0;
            var check = 0;
            if(title && tag)
            {
                // cả 2 cái đều null
                check = 1;
            }
            else if (!title && !tag)
            {
                // cả 2 cái đều ko null
                check = 2;
            }
            else if(!title && tag)
            {
                // chỉ title
                check = 3;
            }
            else if(title && !tag)
            {
                // chỉ tag
                check = 4;
            }

            switch (check)
            {
                default:
                case 1:
                    IQueryable<Post> x = db.postRepository.AllPosts()
                    .Where(m => m.status)
                    .OrderByDescending(m => m.create_date);
                    post =
                    x.Select(m => new lstPostViewModel
                    {
                        post_id = m.post_id,
                        post_title = m.post_title,
                        post_teaser = m.post_teaser,
                        ViewCount = m.ViewCount,
                        AvatarImage = m.AvatarImage,
                        create_date = m.create_date,
                        tagsname = m.Tags.FirstOrDefault().TagName,
                        slug = m.post_slug
                    }
                    ).ToPagedList(pageIndex, pageSize);
                    break;
                case 2:
                    using (DBCongThongTin conn = db.Context)
                    {
                        var query = (
                            // Instance from context
                            from z in taglist
                            // Join list tìm kiếm
                            join a in conn.Tags on z.TagID equals a.TagID
                            // Instance from navigation property
                            from b in a.Posts
                            // Join to bring useful data
                            join c in conn.Posts on b.post_id equals c.post_id
                            where c.status == true
                            // Where c.dynasty == model.Dynasty.ToString()
                            where c.post_title.ToLower().Contains(model.title.ToLower())
                            // Sắp theo 
                            orderby c.Rated
                            select new
                            {
                                c.post_id,
                                c.post_title,
                                c.post_teaser,
                                c.ViewCount,
                                c.AvatarImage,
                                c.create_date,
                                c.Tags.FirstOrDefault().TagName,
                                c.post_slug

                            }).Distinct();
                        // DISTINCT ĐỂ SAU KHI SELECT ĐỐI TƯỢNG MỚI ĐƯỢC
                        // VÌ THẰNG DƯỚI KHÔNG EQUAL HASHCODE
                        post = query.Select(c => new lstPostViewModel
                        {
                            post_id = c.post_id,
                            post_title = c.post_title,
                            post_teaser = c.post_teaser,
                            ViewCount = c.ViewCount,
                            AvatarImage = c.AvatarImage,
                            create_date = c.create_date,
                            tagsname = c.TagName,
                            slug = c.post_slug
                        }).ToPagedList(pageIndex, pageSize);

                    }
                    break;
                case 3:
                    var p = db.postRepository.AllPosts()
                    .Where(m => m.status)
                    .Where(m => m.post_title.Contains(model.title))
                    .OrderBy(m => m.post_title.Contains(model.title));
                    post =
                    p.Select(m => new lstPostViewModel
                    {
                        post_id = m.post_id,
                        post_title = m.post_title,
                        post_teaser = m.post_teaser,
                        ViewCount = m.ViewCount,
                        AvatarImage = m.AvatarImage,
                        create_date = m.create_date,
                        tagsname = m.Tags.FirstOrDefault().TagName,
                        slug = m.post_slug
                    }
                    ).ToPagedList(pageIndex, pageSize);
                    break;
                case 4:
                    using (DBCongThongTin conn = db.Context)
                    {
                        post = (
                            // Instance from context
                            from z in taglist
                            // Join list tìm kiếm
                            join a in conn.Tags on z.TagID equals a.TagID
                            // Instance from navigation property
                            from b in a.Posts
                            // Join to bring useful data
                            join c in conn.Posts on b.post_id equals c.post_id
                            where c.status == true
                            // Sắp theo ngày đăng mới nhất
                            orderby b.create_date descending
                            select new
                            {
                                c.post_id,
                                c.post_title,
                                c.post_teaser,
                                c.ViewCount,
                                c.AvatarImage,
                                c.create_date,
                                c.Tags.FirstOrDefault().TagName,
                                c.post_slug
                            })
                            //DISTINCT ĐỂ SAU KHI SELECT ĐỐI TƯỢNG MỚI ĐƯỢC
                            //VÌ THẰNG DƯỚI KHÔNG EQUAL HASHCODE
                            .Distinct().Select(c => new lstPostViewModel
                            {
                                post_id = c.post_id,
                                post_title = c.post_title,
                                post_teaser = c.post_teaser,
                                ViewCount = c.ViewCount,
                                AvatarImage = c.AvatarImage,
                                create_date = c.create_date,
                                tagsname = c.TagName,
                                slug = c.post_slug
                                
                            })
                            .ToPagedList(pageIndex, pageSize);
                    }
                    break;
               
            }
            
            return View(post);
        }
    }
}