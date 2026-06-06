using Model;
using System;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for notifications: sending, querying, read-state management,
    /// and the teacher/student/lesson-cancelled message helpers.
    /// </summary>
    public static class NotificationLogic
    {
        public static int SendNotification(Notification notification)
        {
            return new NotificationDB().SendNotification(notification);
        }

        public static List<Notification> GetUserNotifications(int userId, string userType)
        {
            return new NotificationDB().GetUserNotifications(userId, userType);
        }

        public static int GetUnreadNotificationCount(int userId, string userType)
        {
            return new NotificationDB().GetUnreadCount(userId, userType);
        }

        public static List<Notification> GetUnreadNotifications(int userId, string userType)
        {
            return new NotificationDB().GetUnreadNotifications(userId, userType);
        }

        public static void MarkNotificationAsRead(int notificationId)
        {
            try
            {
                new NotificationDB().MarkAsRead(notificationId);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "MarkNotificationAsRead failed: " + ex.GetBaseException().Message);
            }
        }

        public static void MarkAllNotificationsAsRead(int userId, string userType)
        {
            try
            {
                new NotificationDB().MarkAllAsRead(userId, userType);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "MarkAllNotificationsAsRead failed: " + ex.GetBaseException().Message);
            }
        }

        public static void DeleteNotification(int notificationId)
        {
            try
            {
                new NotificationDB().DeleteNotification(notificationId);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "DeleteNotification failed: " + ex.GetBaseException().Message);
            }
        }

        public static void SendTeacherMessage(int teacherId, string teacherName, int studentId, string title, string message)
        {
            try
            {
                new NotificationDB().SendTeacherMessage(teacherId, teacherName, studentId, title, message);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "SendTeacherMessage failed: " + ex.GetBaseException().Message);
            }
        }

        public static void SendStudentMessage(int studentId, string studentName, int teacherId, string title, string message)
        {
            try
            {
                new NotificationDB().SendStudentMessage(studentId, studentName, teacherId, title, message);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "SendStudentMessage failed: " + ex.GetBaseException().Message);
            }
        }

        public static void SendLessonCancelledNotification(int studentId, string studentName, int teacherId, string lessonDate, string lessonTime)
        {
            new NotificationDB().SendLessonCancelledNotification(studentId, studentName, teacherId, lessonDate, lessonTime);
        }
    }
}
