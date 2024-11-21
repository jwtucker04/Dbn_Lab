using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DesignDevelopDeploy
{
    public class Student
    {
        public string Name { get; private set; }
        public string Status { get; set; }

        public Student(string name)
        {
            Name = name;
        }

        public void UpdateStatus(string status)
        {
            Status = status;
        }
    }

    public class PersonalSupervisor
    {
        public string Name { get; private set; }
        public List<Student> Students { get; private set; }

        public PersonalSupervisor(string name)
        {
            Name = name;
            Students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            Students.Add(student);
        }

        public List<string> GetStudentNames()
        {
            return Students.Select(s => s.Name).ToList();
        }
    }

    public class StatusManager
    {
        private readonly string _filePath;
        private readonly string _statusFilePath;
        public List<Student> Students { get; private set; } = new List<Student>();
        public List<PersonalSupervisor> Supervisors { get; private set; } = new List<PersonalSupervisor>();

        public StatusManager(string filePath, string statusFilePath)
        {
            _filePath = filePath;
            _statusFilePath = statusFilePath;
            LoadData();
        }

        public bool ST = false;
        private void LoadData()
        {
            using (StreamReader reader = new StreamReader(_filePath))
            {
                string line;
                PersonalSupervisor currentSupervisor = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("PS"))
                    {
                        string supervisorName = ExtractName(line, "Name");
                        currentSupervisor = new PersonalSupervisor(supervisorName);
                        Supervisors.Add(currentSupervisor);
                    }
                    if (currentSupervisor != null)
                    {
                        var studentNames = line.Split("|").Skip(1).Select(s => s.Trim()).ToList();
                        foreach (var studentName in studentNames)
                        {
                            var student = new Student(studentName);
                            Students.Add(student);
                            currentSupervisor.AddStudent(student);
                        }
                    }
                    if (line.Contains("ST"))
                    {
                        ST = true;
                    }
                }
            }
        }

        public void SaveStatus(string name, string status)
        {
            using (StreamWriter writer = new StreamWriter(_statusFilePath, append: true))
            {
                writer.WriteLine($"{name}:{status}");
            }
        }

        public List<string> Statuses = new List<string>();
        public void GetStatus(string name)
        {
            using (StreamReader reader = new StreamReader(_statusFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(name))
                    {
                        Statuses.Add(line);
                    }
                }
            }

        }

        private static string ExtractName(string line, string identifier)
        {
            int startIndex = line.IndexOf(identifier) + identifier.Length + 1;
            int endIndex = line.IndexOf(";", startIndex);
            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }

    public class MeetingManager
    {
        private StatusManager _statusManager;
        string _StatusFilePath;

        public MeetingManager(StatusManager statusManager, string statusFilePath)
        {
            _statusManager = statusManager;
            _StatusFilePath = statusFilePath;
        }

        public bool BookMeeting(string bookerName, bool isStudent)
        {
            if (isStudent)
            {
                var supervisor = FindSupervisorOfStudent(bookerName);
                if (supervisor != null)
                {
                    Console.WriteLine("Book a meeting with " + supervisor.Name + "? (yes/no)");
                    string response = Console.ReadLine();
                    if (response == "yes")
                    {
                        Console.WriteLine("Meeting booked!");
                        using (StreamWriter writer = new StreamWriter(_StatusFilePath, append: true))
                        {
                            writer.WriteLine("Meeting:" + bookerName + "," + supervisor.Name);
                        }
                        return true;
                    }
                }
            }
            else
            {
                var supervisor = _statusManager.Supervisors.FirstOrDefault(s => s.Name == bookerName);
                if (supervisor != null)
                {
                    Console.WriteLine("Which student would you like to book a meeting with?");
                    string studentName = Console.ReadLine();
                    var student = supervisor.Students.FirstOrDefault(s => s.Name == studentName);
                    if (student != null)
                    {
                        Console.WriteLine("Meeting booked!");
                        using (StreamWriter writer = new StreamWriter(_StatusFilePath, append: true))
                        {
                            writer.WriteLine("Meeting:" + bookerName + "," + studentName);
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        private PersonalSupervisor FindSupervisorOfStudent(string studentName)
        {
            return _statusManager.Supervisors.FirstOrDefault(s => s.Students.Any(st => st.Name == studentName));
        }
    }


    internal class ProgramWithClasses
    {
        static void Main(string[] args)
        {
            string filePath = "C:/Users/jwtuc/Documents/DDDData/DDDData.txt";
            string statusFilePath = "C:/Users/jwtuc/Documents/DDDData/StudentStatus.txt";

            Console.WriteLine("Please enter your name:");
            string name = Console.ReadLine();

            StatusManager statusManager = new StatusManager(filePath, statusFilePath);
            MeetingManager meetingManager = new MeetingManager(statusManager, statusFilePath);

            var isStudent = statusManager.Students.Any(s => s.Name == name);
            var isPS = statusManager.Supervisors.Any(s => s.Name == name);

            if (isStudent)
            {
                Console.WriteLine("You are a student.");
                Console.WriteLine("How are you feeling?");
                string status = Console.ReadLine();
                statusManager.SaveStatus(name, status);
                meetingManager.BookMeeting(name, true);

            }
            else if (isPS)
            {

                Console.WriteLine("You are a personal supervisor.");

                Console.WriteLine("\nAll Statuses:\n");

                foreach (PersonalSupervisor ps in statusManager.Supervisors)
                {
                    if (ps.Name == name)
                    {
                        foreach (Student student in ps.Students)
                        {
                            statusManager.GetStatus(student.Name);
                        }
                    }
                }

                foreach (string status in statusManager.Statuses)
                {
                    Console.WriteLine(status);
                }

                meetingManager.BookMeeting(name, false);
            }
            else if (statusManager.ST)
            {
                foreach (var student in statusManager.Students)
                {
                    statusManager.GetStatus(student.Name);
                }
                foreach (string status in statusManager.Statuses)
                {
                    Console.WriteLine(status);
                }
            }
            else
            {
                Console.WriteLine("Unrecognized name.");
            }

        }
    }
}
