using Maha.Microservices.DemoService.Base;
using Maha.Microservices.DemoService.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 学生服务
    /// </summary>
    public class StudentService
    {
        private List<Student> students = new List<Student>();
        private long studentIdNext = 0;

        /// <summary>
        /// 创建学生
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        public long Create(Student student)
        {
            student.Id = ++studentIdNext;
            students.Add(student);
            return studentIdNext;
        }

        /// <summary>
        /// 删除学生
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool Delete(long Id)
        {
            students.RemoveAll(item => item.Id == Id);
            return true;
        }

        /// <summary>
        /// 修改学生
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        public bool Update(Student student)
        {
            var oldIndex = students.FindIndex(item => item.Id == student.Id);
            var oldStudent = students[oldIndex];
            if (oldStudent == null)
                throw new RpcException($"找不到学生：{oldStudent.Id}");
            return true;
        }

        /// <summary>
        /// 查询学生
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Student> QueryStudent(StudentQueryFilter filter)
        {
            return students.Where((item) =>
            {
                if (filter.Id != 0)
                {
                    return item.Id == filter.Id;
                }
                else if (string.IsNullOrWhiteSpace(filter.Name))
                {
                    return item.Name.Contains(filter.Name);
                }
                else
                {
                    return true;
                }
            }).ToList();
        }
    }
}
