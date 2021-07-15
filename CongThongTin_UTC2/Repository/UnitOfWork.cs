using CongThongTin_UTC2.Models;
using CongThongTin_UTC2.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2
{
    public class UnitOfWork
    {
        private DBCongThongTin context = new DBCongThongTin();
        private UserRepository _userRepository;
        private InfoRepository _infoRepository;
        private PostRepository _postRepository;
        private SeriesRepository _seriesRepository;
        private TagRepository _tagRepository;
        private HotPostRepository _hotPostRepository;
        private CommentRepository _commentRepository;
        public UnitOfWork(DBCongThongTin _context)
        {
            this.context = _context;
        }
        public DBCongThongTin Context
        {
            get
            {
                return context;
            }
        }
        public HotPostRepository hotPostRepository
        {
            get
            {
                if (_hotPostRepository == null)
                {
                    _hotPostRepository = new HotPostRepository(context);

                }
                return _hotPostRepository;
            }
        }
        public TagRepository tagRepository
        {
            get
            {
                if (_tagRepository == null)
                {
                    _tagRepository = new TagRepository(context);

                }
                return _tagRepository;
            }
        }
        public SeriesRepository seriesRepository
        {
            get
            {
                if (_seriesRepository == null)
                {
                    _seriesRepository = new SeriesRepository(context);

                }
                return _seriesRepository;
            }
        }
        public PostRepository postRepository
        {
            get
            {
                if(_postRepository== null)
                {
                    _postRepository = new PostRepository(context);
                    
                }
                return _postRepository;
            }
        }
        public InfoRepository infoRepository
        {
            get
            {
                if (_infoRepository == null)
                {
                    _infoRepository = new InfoRepository(context);

                }
                return _infoRepository;
            }
        }
        public UserRepository userRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(context);

                }
                return _userRepository;
            }
        }
        public CommentRepository commentRepository
        {
            get
            {
                if (_commentRepository == null)
                {
                    _commentRepository = new CommentRepository(context);

                }
                return _commentRepository;
            }
        }
        public void Commit()
        {
            context.SaveChanges();
        }
    }
}