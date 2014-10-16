using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Tyvj.DataModels
{
    public class DB : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<ContestProblem> ContestProblems { get; set; }
        public DbSet<AlgorithmTag> AlgorithmTags { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<SolutionTag> SolutionTags { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<JudgeTask> JudgeTasks { get; set; }
        public DbSet<ContestRegister> ContestRegisters { get; set; }
        public DbSet<Lock> Locks { get; set; }
        public DB()
            : base("mysqldb")
        { 
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}