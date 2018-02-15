﻿using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using EventStatistic;
using JetBrains.Annotations;
using NetLib;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AcadLib.Statistic
{
    public static class EventsStatisticService
    {
        private static Eventer eventer;
        private static Database db;

        public static void Start()
        {
            eventer = new Eventer("AutoCAD", HostApplicationServices.Current.releaseMarketVersion);
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
            try
            {
                SubscribeDoc(AcadHelper.Doc);
            }
            catch
            {
                //
            }
        }

        private static void DocumentManager_DocumentActivated(object sender, [NotNull] DocumentCollectionEventArgs e)
        {
            SubscribeDoc(e.Document);
        }

        private static void SubscribeDoc([NotNull] Document doc)
        {
            if (db != null)
            {
                db.SaveComplete -= Db_SaveComplete;
                db.BeginSave -= Db_BeginSave;
            }
            db = doc.Database;
            db.SaveComplete += Db_SaveComplete;
            db.BeginSave += Db_BeginSave;
        }

        private static void Db_BeginSave(object sender, DatabaseIOEventArgs e)
        {
            try
            {
                eventer.Start();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"Ошибка EventsStatistic Start '{e.FileName}'");
            }
        }

        private static void Db_SaveComplete(object sender, [NotNull] DatabaseIOEventArgs e)
        {
            try
            {
                var res = eventer.Finish("Сохранить", e.FileName);
                if (!res.IsNullOrEmpty())
                {
                    Logger.Log.Error($"Ошибка EventsStatistic Finish Result '{e.FileName}' - {res}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex ,$"Ошибка EventsStatistic Finish '{e.FileName}'");
            }
        }
    }
}