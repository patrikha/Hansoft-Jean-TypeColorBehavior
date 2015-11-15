using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using HPMSdk;
using Hansoft.ObjectWrapper;
using Hansoft.ObjectWrapper.CustomColumnValues;

using Hansoft.Jean.Behavior;

namespace Hansoft.Jean.Behavior.TypeColorBehavior
{
    public class TypeColorBehavior : AbstractBehavior
    {
        public static bool debug = false;
        string projectName;
        bool initializationOK = false;
        string viewName;
        EHPMReportViewType viewType;
        List<Project> projects;
        private List<ProjectView> projectViews;
        bool inverted = false;
        string title;

        public TypeColorBehavior(XmlElement configuration)
            : base(configuration)
        {
            projectName = GetParameter("HansoftProject");
            string invert = GetParameter("InvertedMatch");
            if (invert != null)
                inverted = invert.ToLower().Equals("yes");
            viewName = GetParameter("View");
            viewType = GetViewType(viewName);
            title = "TypeColorBehavior: " + configuration.InnerText;
        }

        public override void Initialize()
        {
            projects = new List<Project>();
            projectViews = new List<ProjectView>();
            initializationOK = false;
            projects = HPMUtilities.FindProjects(projectName, inverted);
            if (projects.Count == 0)
                throw new ArgumentException("Could not find any matching project:" + projectName);
            foreach (Project project in projects)
            {
                ProjectView projectView;
                if (viewType == EHPMReportViewType.AgileBacklog)
                    projectView = project.ProductBacklog;
                else if (viewType == EHPMReportViewType.AllBugsInProject)
                    projectView = project.BugTracker;
                else
                    projectView = project.Schedule;
                projectViews.Add(projectView);
            }
            initializationOK = true;
            DoUpdate();
        }

        public override string Title
        {
            get { return title; }
        }

        private void DoUpdate()
        {
            if (initializationOK)
            {
                foreach (ProjectView view in projectViews)
                {
                    foreach (Task child in view.Find(""))
                    {
                        var color = GetColor(child);
                        if (color == null || child.WallItemColor == color)
                            continue;
                        child.WallItemColor = (EHPMTaskWallItemColor)color;
                    }
                }
            }
        }

        // Entry point if custom data is changed
        public override void OnTaskChangeCustomColumnData(TaskChangeCustomColumnDataEventArgs e)
        {
            if (initializationOK)
            {
                Task task = Task.GetTask(e.Data.m_TaskID);
                if (projects.Contains(task.Project) && projectViews.Contains(task.ProjectView))
                {
                    var color = GetColor(task);
                    if (color == null)
                        return;
                    task.WallItemColor = (EHPMTaskWallItemColor)color;
                }
            }
        }

        private EHPMTaskWallItemColor? GetColor(Task task)
        {
            var type = task.GetCustomColumnValue("Type");
            if (type == null)
                return null;
            var color = TypeColorConfig.GetTypeColor(type.ToString());
            if (color == null)
                return EHPMTaskWallItemColor.Orange;
            switch (color)
            { 
                case "Yellow":
                    return EHPMTaskWallItemColor.Yellow;
                case "Blue":
                    return EHPMTaskWallItemColor.Blue;
                case "Green":
                    return EHPMTaskWallItemColor.Green;
                case "Red":
                    return EHPMTaskWallItemColor.Red;
                case "Magenta":
                    return EHPMTaskWallItemColor.Magenta;
                case "Orange":
                    return EHPMTaskWallItemColor.Orange;
            }
            return null;
        }
    }
}

