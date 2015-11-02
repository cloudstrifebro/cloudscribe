﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:				    2007-11-03
// Last Modified:			2015-11-02
// 

using cloudscribe.DbHelpers.pgsql;
using Microsoft.Framework.Logging;
using Npgsql;
using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;


namespace cloudscribe.Core.Repositories.pgsql
{

    internal class DBSiteUser
    {
        internal DBSiteUser(
            string dbReadConnectionString,
            string dbWriteConnectionString,
            ILoggerFactory loggerFactory)
        {
            logFactory = loggerFactory;
            readConnectionString = dbReadConnectionString;
            writeConnectionString = dbWriteConnectionString;
        }

        private ILoggerFactory logFactory;
        //private ILogger log;
        private string readConnectionString;
        private string writeConnectionString;

        public DbDataReader GetUserCountByYearMonth(int siteId)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT ");
            sqlCommand.Append("cast(date_part('year', datecreated) as int4) As y,  ");
            sqlCommand.Append("cast(date_part('month', datecreated) as int4) As m, ");
            sqlCommand.Append("cast(date_part('year', datecreated) as varchar(10)) || '-' || cast(date_part('month', datecreated) as varchar(3))  As label, ");
            sqlCommand.Append("COUNT(*) As users ");

            sqlCommand.Append("FROM ");
            sqlCommand.Append("mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("GROUP BY cast(date_part('year', datecreated) as int4), cast(date_part('month', datecreated) as int4), cast(date_part('year', datecreated) as varchar(10)) || '-' || cast(date_part('month', datecreated) as varchar(3)) ");
            sqlCommand.Append("ORDER BY cast(date_part('year', datecreated) as int4), cast(date_part('month', datecreated) as int4) ");
            sqlCommand.Append("; ");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }


        public DbDataReader GetUserList(int siteId)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT UserID, ");
            sqlCommand.Append("name, ");
            sqlCommand.Append("passwordsalt, ");
            sqlCommand.Append("pwd, ");
            sqlCommand.Append("email ");
            sqlCommand.Append("FROM mp_users ");

            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("ORDER BY ");
            sqlCommand.Append("email");
            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);



        }


        public DbDataReader GetSmartDropDownData(int siteId, string query, int rowsToGet)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("query", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = query + "%";

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT ");

            sqlCommand.Append("userid, ");
            sqlCommand.Append("userguid, ");
            sqlCommand.Append("firstname, ");
            sqlCommand.Append("lastname, ");
            sqlCommand.Append("email, ");
            sqlCommand.Append("name AS siteuser ");

            sqlCommand.Append("FROM mp_users ");

            sqlCommand.Append("WHERE siteid = :siteid ");
            sqlCommand.Append("AND isdeleted = false ");
            sqlCommand.Append("AND (  ");
            sqlCommand.Append(" (name LIKE :query) ");
            sqlCommand.Append(" OR (firstname LIKE :query) ");
            sqlCommand.Append(" OR (lastname LIKE :query) ");
            sqlCommand.Append(") ");

            sqlCommand.Append("UNION ");

            sqlCommand.Append("SELECT ");

            sqlCommand.Append("userid, ");
            sqlCommand.Append("userguid, ");
            sqlCommand.Append("firstname, ");
            sqlCommand.Append("lastname, ");
            sqlCommand.Append("email, ");
            sqlCommand.Append("email As siteuser ");

            sqlCommand.Append("FROM mp_users ");

            sqlCommand.Append("WHERE siteid = :siteid ");
            sqlCommand.Append("AND isdeleted = false ");
            sqlCommand.Append("AND email LIKE :query   ");

            sqlCommand.Append("ORDER BY siteuser ");

            sqlCommand.Append("LIMIT " + rowsToGet.ToString());

            sqlCommand.Append(";");

            return AdoHelper.ExecuteReader(
               readConnectionString,
               CommandType.Text,
               sqlCommand.ToString(),
               arParams);



        }

        public DbDataReader EmailLookup(int siteId, string query, int rowsToGet)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("query", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = query + "%";

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT ");

            sqlCommand.Append("userid, ");
            sqlCommand.Append("userguid, ");
            sqlCommand.Append("email ");

            sqlCommand.Append("FROM mp_users ");

            sqlCommand.Append("WHERE siteid = :siteid ");
            sqlCommand.Append("AND isdeleted = false ");
            sqlCommand.Append("AND (  ");
            sqlCommand.Append(" (email LIKE :query) ");
            sqlCommand.Append("OR (name LIKE :query) ");
            sqlCommand.Append(" OR (firstname LIKE :query) ");
            sqlCommand.Append(" OR (lastname LIKE :query) ");
            sqlCommand.Append(") ");

            sqlCommand.Append("ORDER BY email ");

            sqlCommand.Append("LIMIT " + rowsToGet.ToString());

            sqlCommand.Append(";");

            return AdoHelper.ExecuteReader(
               readConnectionString,
               CommandType.Text,
               sqlCommand.ToString(),
               arParams);
        }






        public int UserCount(int siteId)
        {

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  Count(*) ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append(";");

            //int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_count(:siteid)", 
            //    arParams));

            int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return count;

        }

        public async Task<int> CountUsers(int siteId, String userNameBeginsWith)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("usernamebeginswith", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = userNameBeginsWith + "%";

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  Count(*) ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("name like :usernamebeginswith ");
            sqlCommand.Append(";");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_countbyfirstletter(:siteid,:usernamebeginswith)", 
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int count = Convert.ToInt32(result);

            return count;

        }


        public int CountUsersByRegistrationDateRange(
            int siteId,
            DateTime beginDate,
            DateTime endDate)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[3];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("begindate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = beginDate;

            arParams[2] = new NpgsqlParameter("enddate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[2].Value = endDate;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  Count(*) ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("datecreated >= :begindate ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("datecreated < :enddate ");
            sqlCommand.Append(";");

            //int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_countbyregistrationdaterange(:siteid,:begindate,:enddate)", 
            //    arParams));

            int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return count;

        }


        public int CountOnlineSince(int siteId, DateTime sinceTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("sincetime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = sinceTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  Count(*) ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("lastactivitydate > :sincetime ");
            sqlCommand.Append(";");

            //int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_countonlinesince(:siteid,:sincetime)", 
            //    arParams));

            int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return count;

        }

        public DbDataReader GetUsersOnlineSince(int siteId, DateTime sinceTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("sincetime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = sinceTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("lastactivitydate > :sincetime ");
            sqlCommand.Append(";");

            //return AdoHelper.ExecuteReader(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_getusersonlinesince(:siteid,:sincetime)",
            //    arParams);

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public DbDataReader GetTop50UsersOnlineSince(int siteId, DateTime sinceTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("sincetime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = sinceTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("lastactivitydate > :sincetime ");
            sqlCommand.Append("LIMIT 50");
            sqlCommand.Append(";");

            //return AdoHelper.ExecuteReader(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_gettopusersonlinesince(:siteid,:sincetime)",
            //    arParams);

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);



        }

        public async Task<int> GetNewestUserId(int siteId)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  cast(max(userid) as int4) ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append(";");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_getnewestid(:siteid)", 
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int count = Convert.ToInt32(result);

            return count;

        }


        public async Task<DbDataReader> GetUserListPage(
            int siteId,
            int pageNumber,
            int pageSize,
            string userNameBeginsWith,
            int sortMode)
        {
            int pageLowerBound = (pageSize * pageNumber) - pageSize;

            NpgsqlParameter[] arParams = new NpgsqlParameter[4];

            arParams[0] = new NpgsqlParameter("pageoffset", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = pageLowerBound;

            arParams[1] = new NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = pageSize;

            arParams[2] = new NpgsqlParameter("usernamebeginswith", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[2].Value = userNameBeginsWith + "%";

            arParams[3] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[3].Value = siteId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT	u.* ");

            sqlCommand.Append("FROM	mp_users u ");

            sqlCommand.Append("WHERE u.profileapproved = true   ");
            sqlCommand.Append("AND u.siteid = :siteid   ");

            if (userNameBeginsWith.Length > 0)
            {
                sqlCommand.Append(" AND u.name LIKE :usernamebeginswith ");
            }

            switch (sortMode)
            {
                case 1:
                    sqlCommand.Append(" ORDER BY u.datecreated DESC ");
                    break;

                case 2:
                    sqlCommand.Append(" ORDER BY u.lastname, u.firstname, u.name ");
                    break;

                case 0:
                default:
                    sqlCommand.Append(" ORDER BY u.name ");
                    break;
            }

            sqlCommand.Append("LIMIT  :pagesize");

            if (pageNumber > 1)
                sqlCommand.Append(" OFFSET :pageoffset ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<int> CountUsersForSearch(int siteId, string searchInput)
        {

            StringBuilder sqlCommand = new StringBuilder();

            sqlCommand.Append("SELECT Count(*) FROM mp_users WHERE siteid = :siteid ");
            sqlCommand.Append("AND profileapproved = true ");
            sqlCommand.Append("AND displayinmemberlist = true ");
            sqlCommand.Append("AND isdeleted = false ");

            if (searchInput.Length > 0)
            {
                sqlCommand.Append(" AND ");
                sqlCommand.Append("(");

                sqlCommand.Append(" (name LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (loginname LIKE :searchinput) ");
                //sqlCommand.Append(" OR ");
                //sqlCommand.Append(" (email LIKE :searchinput) ");

                sqlCommand.Append(")");

            }
            sqlCommand.Append(" ;  ");


            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("searchinput", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = "%" + searchInput + "%";

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return Convert.ToInt32(result);


        }

        public async Task<DbDataReader> GetUserSearchPage(
            int siteId,
            int pageNumber,
            int pageSize,
            string searchInput,
            int sortMode)
        {
            int pageLowerBound = (pageSize * pageNumber) - pageSize;

            NpgsqlParameter[] arParams = new NpgsqlParameter[4];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("searchinput", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = "%" + searchInput + "%";

            arParams[2] = new NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[2].Value = pageSize;

            arParams[3] = new NpgsqlParameter("pageoffset", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[3].Value = pageLowerBound;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT	* ");
            sqlCommand.Append("FROM	mp_users  ");

            sqlCommand.Append("WHERE  ");
            sqlCommand.Append("siteid = :siteid  ");

            sqlCommand.Append("AND profileapproved = true ");
            sqlCommand.Append("AND displayinmemberlist = true ");
            sqlCommand.Append("AND isdeleted = false ");

            if (searchInput.Length > 0)
            {
                sqlCommand.Append(" AND ");
                sqlCommand.Append("(");

                sqlCommand.Append(" (name LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (loginname LIKE :searchinput) ");
                //sqlCommand.Append(" OR ");
                //sqlCommand.Append(" (email LIKE :searchinput) ");

                sqlCommand.Append(")");

            }

            switch (sortMode)
            {
                case 1:
                    sqlCommand.Append(" ORDER BY datecreated DESC ");
                    break;

                case 2:
                    sqlCommand.Append(" ORDER BY lastname, firstname, name ");
                    break;

                case 0:
                default:
                    sqlCommand.Append(" ORDER BY name ");
                    break;
            }

            sqlCommand.Append("LIMIT  :pagesize");

            if (pageNumber > 1)
                sqlCommand.Append(" OFFSET :pageoffset ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<int> CountUsersForAdminSearch(int siteId, string searchInput)
        {
            StringBuilder sqlCommand = new StringBuilder();

            sqlCommand.Append("SELECT Count(*) FROM mp_users WHERE siteid = :siteid ");
            if (searchInput.Length > 0)
            {
                sqlCommand.Append(" AND ");
                sqlCommand.Append("(");

                sqlCommand.Append(" (name LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (loginname LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (email LIKE :searchinput) ");

                sqlCommand.Append(")");

            }
            sqlCommand.Append(" ;  ");


            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("searchinput", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = "%" + searchInput + "%";

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return Convert.ToInt32(result);

        }

        public async Task<DbDataReader> GetUserAdminSearchPage(
            int siteId,
            int pageNumber,
            int pageSize,
            string searchInput,
            int sortMode)
        {
            int pageLowerBound = (pageSize * pageNumber) - pageSize;

            NpgsqlParameter[] arParams = new NpgsqlParameter[4];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("searchinput", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[1].Value = "%" + searchInput + "%";

            arParams[2] = new NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[2].Value = pageSize;

            arParams[3] = new NpgsqlParameter("pageoffset", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[3].Value = pageLowerBound;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT	* ");
            sqlCommand.Append("FROM	mp_users  ");

            sqlCommand.Append("WHERE  ");
            sqlCommand.Append("siteid = :siteid  ");

            if (searchInput.Length > 0)
            {
                sqlCommand.Append(" AND ");
                sqlCommand.Append("(");
                sqlCommand.Append(" (name LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (loginname LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (email LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (lastname LIKE :searchinput) ");
                sqlCommand.Append(" OR ");
                sqlCommand.Append(" (firstname LIKE :searchinput) ");
                sqlCommand.Append(")");

            }

            switch (sortMode)
            {
                case 1:
                    sqlCommand.Append(" ORDER BY datecreated DESC ");
                    break;

                case 2:
                    sqlCommand.Append(" ORDER BY lastname, firstname, name ");
                    break;

                case 0:
                default:
                    sqlCommand.Append(" ORDER BY name ");
                    break;
            }

            sqlCommand.Append("LIMIT  :pagesize");

            if (pageNumber > 1)
                sqlCommand.Append(" OFFSET :pageoffset ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);


        }

        public async Task<int> CountLockedOutUsers(int siteId)
        {
            StringBuilder sqlCommand = new StringBuilder();

            sqlCommand.Append("SELECT Count(*) FROM mp_users WHERE siteid = :siteid AND islockedout = true; ");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return Convert.ToInt32(result);

        }

        public async Task<DbDataReader> GetPageLockedUsers(
            int siteId,
            int pageNumber,
            int pageSize)
        {
            int pageLowerBound = (pageSize * pageNumber) - pageSize;

            NpgsqlParameter[] arParams = new NpgsqlParameter[3];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = pageSize;

            arParams[2] = new NpgsqlParameter("pageoffset", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[2].Value = pageLowerBound;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT	* ");
            sqlCommand.Append("FROM	mp_users  ");

            sqlCommand.Append("WHERE  ");
            sqlCommand.Append("siteid = :siteid  ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("islockedout = true ");

            sqlCommand.Append("ORDER BY  ");
            sqlCommand.Append("name  ");
            sqlCommand.Append("LIMIT  :pagesize");

            if (pageNumber > 1)
                sqlCommand.Append(" OFFSET :pageoffset ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }



        public async Task<int> CountNotApprovedUsers(int siteId)
        {
            StringBuilder sqlCommand = new StringBuilder();

            sqlCommand.Append("SELECT Count(*) FROM mp_users WHERE siteid = :siteid AND approvedforforums = false; ");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            object result = await AdoHelper.ExecuteScalarAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return Convert.ToInt32(result);
        }

        public async Task<DbDataReader> GetPageNotApprovedUsers(
            int siteId,
            int pageNumber,
            int pageSize)
        {
            int pageLowerBound = (pageSize * pageNumber) - pageSize;

            NpgsqlParameter[] arParams = new NpgsqlParameter[3];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = pageSize;

            arParams[2] = new NpgsqlParameter("pageoffset", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[2].Value = pageLowerBound;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT	* ");
            sqlCommand.Append("FROM	mp_users  ");

            sqlCommand.Append("WHERE  ");
            sqlCommand.Append("siteid = :siteid  ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("approvedforforums = false ");

            sqlCommand.Append("ORDER BY  ");
            sqlCommand.Append("name  ");
            sqlCommand.Append("LIMIT  :pagesize");

            if (pageNumber > 1)
                sqlCommand.Append(" OFFSET :pageoffset ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public int AddUser(
            Guid siteGuid,
            int siteId,
            string fullName,
            String loginName,
            string email,
            string password,
            string passwordSalt,
            Guid userGuid,
            DateTime dateCreated,
            bool mustChangePwd,
            string firstName,
            string lastName,
            string timeZoneId,
            DateTime dateOfBirth,
            bool emailConfirmed,
            int pwdFormat,
            string passwordHash,
            string securityStamp,
            string phoneNumber,
            bool phoneNumberConfirmed,
            bool twoFactorEnabled,
            DateTime? lockoutEndDateUtc)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("INSERT INTO mp_users (");
            sqlCommand.Append("siteid, ");
            sqlCommand.Append("siteguid, ");
            sqlCommand.Append("name, ");
            sqlCommand.Append("loginname, ");
            sqlCommand.Append("email, ");
            sqlCommand.Append("loweredemail, ");

            sqlCommand.Append("firstname, ");
            sqlCommand.Append("lastname, ");
            sqlCommand.Append("timezoneid, ");
            sqlCommand.Append("emailchangeguid, ");
            sqlCommand.Append("passwordresetguid, ");
            sqlCommand.Append("password, ");
            sqlCommand.Append("pwd, ");
            sqlCommand.Append("passwordsalt, ");
            sqlCommand.Append("mustchangepwd, ");
            sqlCommand.Append("roleschanged, ");
            sqlCommand.Append("totalposts, ");
            sqlCommand.Append("datecreated, ");
            sqlCommand.Append("userguid, ");
            sqlCommand.Append("dateofbirth, ");

            sqlCommand.Append("emailconfirmed, ");
            sqlCommand.Append("pwdformat, ");
            sqlCommand.Append("passwordhash, ");
            sqlCommand.Append("securitystamp, ");
            sqlCommand.Append("phonenumber, ");
            sqlCommand.Append("phonenumberconfirmed, ");
            sqlCommand.Append("twofactorenabled, ");
            sqlCommand.Append("lockoutenddateutc, ");

            sqlCommand.Append("totalrevenue )");

            sqlCommand.Append(" VALUES (");
            sqlCommand.Append(":siteid, ");
            sqlCommand.Append(":siteguid, ");
            sqlCommand.Append(":name, ");
            sqlCommand.Append(":loginname, ");
            sqlCommand.Append(":email, ");
            sqlCommand.Append(":loweredemail, ");

            sqlCommand.Append(":firstname, ");
            sqlCommand.Append(":lastname, ");
            sqlCommand.Append(":timezoneid, ");
            sqlCommand.Append(":emailchangeguid, ");
            sqlCommand.Append("'00000000-0000-0000-0000-000000000000', ");

            sqlCommand.Append("'not used', "); // legacy password field cannot be null
            sqlCommand.Append(":password, ");
            sqlCommand.Append(":passwordsalt, ");
            sqlCommand.Append(":mustchangepwd, ");
            sqlCommand.Append("false, ");
            sqlCommand.Append("0, ");
            sqlCommand.Append(":datecreated, ");
            sqlCommand.Append(":userguid, ");
            sqlCommand.Append(":dateofbirth, ");

            sqlCommand.Append(":emailconfirmed, ");
            sqlCommand.Append(":pwdformat, ");
            sqlCommand.Append(":passwordhash, ");
            sqlCommand.Append(":securitystamp, ");
            sqlCommand.Append(":phonenumber, ");
            sqlCommand.Append(":phonenumberconfirmed, ");
            sqlCommand.Append(":twofactorenabled, ");
            sqlCommand.Append(":lockoutenddateutc, ");

            sqlCommand.Append("0 )");
            sqlCommand.Append(";");
            sqlCommand.Append(" SELECT CURRVAL('mp_users_userid_seq');");

            NpgsqlParameter[] arParams = new NpgsqlParameter[24];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[1].Value = fullName;

            arParams[2] = new NpgsqlParameter("loginname", NpgsqlTypes.NpgsqlDbType.Text, 50);
            arParams[2].Value = loginName;

            arParams[3] = new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[3].Value = email;

            arParams[4] = new NpgsqlParameter("password", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[4].Value = password.ToString();

            arParams[5] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Text, 36);
            arParams[5].Value = userGuid.ToString();

            arParams[6] = new NpgsqlParameter("datecreated", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[6].Value = dateCreated;

            arParams[7] = new NpgsqlParameter("siteguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[7].Value = siteGuid.ToString();

            arParams[8] = new NpgsqlParameter("loweredemail", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[8].Value = email.ToLower();

            arParams[9] = new NpgsqlParameter("mustchangepwd", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[9].Value = mustChangePwd;

            arParams[10] = new NpgsqlParameter("firstname", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[10].Value = firstName;

            arParams[11] = new NpgsqlParameter("lastname", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[11].Value = lastName;

            arParams[12] = new NpgsqlParameter("timezoneid", NpgsqlTypes.NpgsqlDbType.Text, 32);
            arParams[12].Value = timeZoneId;

            arParams[13] = new NpgsqlParameter("emailchangeguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[13].Value = Guid.Empty.ToString();

            arParams[14] = new NpgsqlParameter("passwordsalt", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[14].Value = passwordSalt;

            arParams[15] = new NpgsqlParameter("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp);
            if (dateOfBirth == DateTime.MinValue)
            {
                arParams[15].Value = DBNull.Value;
            }
            else
            {
                arParams[15].Value = dateOfBirth;
            }

            arParams[16] = new NpgsqlParameter("emailconfirmed", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[16].Value = emailConfirmed;

            arParams[17] = new NpgsqlParameter("pwdformat", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[17].Value = pwdFormat;

            arParams[18] = new NpgsqlParameter("passwordhash", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[18].Value = passwordHash;

            arParams[19] = new NpgsqlParameter("securitystamp", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[19].Value = securityStamp;

            arParams[20] = new NpgsqlParameter("phonenumber", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[20].Value = phoneNumber;

            arParams[21] = new NpgsqlParameter("phonenumberconfirmed", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[21].Value = phoneNumberConfirmed;

            arParams[22] = new NpgsqlParameter("twofactorenabled", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[22].Value = twoFactorEnabled;

            arParams[23] = new NpgsqlParameter("lockoutenddateutc", NpgsqlTypes.NpgsqlDbType.Timestamp);
            if (lockoutEndDateUtc == null)
            {
                arParams[23].Value = DBNull.Value;
            }
            else
            {
                arParams[23].Value = lockoutEndDateUtc;
            }

            int newID = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return newID;
        }

        public async Task<bool> UpdateUser(
            int userId,
            String fullName,
            String loginName,
            String email,
            String password,
            string passwordSalt,
            String gender,
            bool profileApproved,
            bool approvedForForums,
            bool trusted,
            bool displayInMemberList,
            String webSiteUrl,
            String country,
            String state,
            String occupation,
            String interests,
            String msn,
            String yahoo,
            String aim,
            String icq,
            String avatarUrl,
            String signature,
            String skin,
            String loweredEmail,
            String passwordQuestion,
            String passwordAnswer,
            String comment,
            int timeOffsetHours,
            string openIdUri,
            string windowsLiveId,
            bool mustChangePwd,
            string firstName,
            string lastName,
            string timeZoneId,
            string editorPreference,
            string newEmail,
            Guid emailChangeGuid,
            Guid passwordResetGuid,
            bool rolesChanged,
            string authorBio,
            DateTime dateOfBirth,
            bool emailConfirmed,
            int pwdFormat,
            string passwordHash,
            string securityStamp,
            string phoneNumber,
            bool phoneNumberConfirmed,
            bool twoFactorEnabled,
            DateTime? lockoutEndDateUtc)
        {

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET email = :email ,   ");
            sqlCommand.Append("name = :name,    ");
            sqlCommand.Append("loginname = :loginname,    ");

            sqlCommand.Append("firstname = :firstname,    ");
            sqlCommand.Append("lastname = :lastname,    ");
            sqlCommand.Append("timezoneid = :timezoneid,    ");
            sqlCommand.Append("editorpreference = :editorpreference,    ");
            sqlCommand.Append("newemail = :newemail,    ");
            sqlCommand.Append("emailchangeguid = :emailchangeguid,    ");
            sqlCommand.Append("passwordresetguid = :passwordresetguid,    ");

            sqlCommand.Append("pwd = :password,    ");
            sqlCommand.Append("passwordsalt = :passwordsalt,    ");
            sqlCommand.Append("mustchangepwd = :mustchangepwd,    ");
            sqlCommand.Append("roleschanged = :roleschanged,    ");
            sqlCommand.Append("gender = :gender,    ");
            sqlCommand.Append("profileapproved = :profileapproved,    ");
            sqlCommand.Append("approvedforforums = :approvedforforums,    ");
            sqlCommand.Append("trusted = :trusted,    ");
            sqlCommand.Append("displayinmemberlist = :displayinmemberlist,    ");
            sqlCommand.Append("websiteurl = :websiteurl,    ");
            sqlCommand.Append("country = :country,    ");
            sqlCommand.Append("state = :state,    ");
            sqlCommand.Append("occupation = :occupation,    ");
            sqlCommand.Append("interests = :interests,    ");
            sqlCommand.Append("msn = :msn,    ");
            sqlCommand.Append("yahoo = :yahoo,   ");
            sqlCommand.Append("aim = :aim,   ");
            sqlCommand.Append("icq = :icq,    ");
            sqlCommand.Append("avatarurl = :avatarurl,    ");
            sqlCommand.Append("signature = :signature,    ");
            sqlCommand.Append("skin = :skin,    ");
            sqlCommand.Append("authorbio = :authorbio,    ");
            sqlCommand.Append("loweredemail = :loweredemail,    ");
            sqlCommand.Append("passwordquestion = :passwordquestion,    ");
            sqlCommand.Append("passwordanswer = :passwordanswer,    ");
            sqlCommand.Append("comment = :comment,    ");
            sqlCommand.Append("openiduri = :openiduri,    ");
            sqlCommand.Append("windowsliveid = :windowsliveid, ");
            sqlCommand.Append("dateofbirth = :dateofbirth, ");

            sqlCommand.Append("emailconfirmed = :emailconfirmed, ");
            sqlCommand.Append("pwdformat = :pwdformat, ");
            sqlCommand.Append("passwordhash = :passwordhash, ");
            sqlCommand.Append("securitystamp = :securitystamp, ");
            sqlCommand.Append("phonenumber = :phonenumber, ");
            sqlCommand.Append("phonenumberconfirmed = :phonenumberconfirmed, ");
            sqlCommand.Append("twofactorenabled = :twofactorenabled, ");
            sqlCommand.Append("lockoutenddateutc = :lockoutenddateutc, ");

            sqlCommand.Append("timeoffsethours = :timeoffsethours    ");

            sqlCommand.Append("WHERE userid = :userid ;");

            NpgsqlParameter[] arParams = new NpgsqlParameter[49];

            arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = userId;

            arParams[1] = new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[1].Value = fullName;

            arParams[2] = new NpgsqlParameter("loginname", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[2].Value = loginName;

            arParams[3] = new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[3].Value = email;

            arParams[4] = new NpgsqlParameter("loweredemail", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[4].Value = loweredEmail;

            arParams[5] = new NpgsqlParameter("password", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[5].Value = password;

            arParams[6] = new NpgsqlParameter("passwordquestion", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[6].Value = passwordQuestion;

            arParams[7] = new NpgsqlParameter("passwordanswer", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[7].Value = passwordAnswer;

            arParams[8] = new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Text, 10);
            arParams[8].Value = gender;

            arParams[9] = new NpgsqlParameter("profileapproved", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[9].Value = profileApproved;

            arParams[10] = new NpgsqlParameter("approvedforforums", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[10].Value = approvedForForums;

            arParams[11] = new NpgsqlParameter("trusted", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[11].Value = trusted;

            arParams[12] = new NpgsqlParameter("displayinmemberlist", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[12].Value = displayInMemberList;

            arParams[13] = new NpgsqlParameter("websiteurl", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[13].Value = webSiteUrl;

            arParams[14] = new NpgsqlParameter("country", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[14].Value = country;

            arParams[15] = new NpgsqlParameter("state", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[15].Value = state;

            arParams[16] = new NpgsqlParameter("occupation", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[16].Value = occupation;

            arParams[17] = new NpgsqlParameter("interests", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[17].Value = interests;

            arParams[18] = new NpgsqlParameter("msn", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[18].Value = msn;

            arParams[19] = new NpgsqlParameter("yahoo", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[19].Value = yahoo;

            arParams[20] = new NpgsqlParameter("aim", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[20].Value = aim;

            arParams[21] = new NpgsqlParameter("icq", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[21].Value = icq;

            arParams[22] = new NpgsqlParameter("avatarurl", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[22].Value = avatarUrl;

            arParams[23] = new NpgsqlParameter("timeoffsethours", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[23].Value = timeOffsetHours;

            arParams[24] = new NpgsqlParameter("signature", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[24].Value = signature;

            arParams[25] = new NpgsqlParameter("skin", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[25].Value = skin;

            arParams[26] = new NpgsqlParameter("comment", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[26].Value = comment;

            arParams[27] = new NpgsqlParameter("openiduri", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[27].Value = openIdUri;

            arParams[28] = new NpgsqlParameter("windowsliveid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[28].Value = windowsLiveId;

            arParams[29] = new NpgsqlParameter("mustchangepwd", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[29].Value = mustChangePwd;

            arParams[30] = new NpgsqlParameter("firstname", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[30].Value = firstName;

            arParams[31] = new NpgsqlParameter("lastname", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[31].Value = lastName;

            arParams[32] = new NpgsqlParameter("timezoneid", NpgsqlTypes.NpgsqlDbType.Varchar, 32);
            arParams[32].Value = timeZoneId;

            arParams[33] = new NpgsqlParameter("editorpreference", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[33].Value = editorPreference;

            arParams[34] = new NpgsqlParameter("newemail", NpgsqlTypes.NpgsqlDbType.Varchar, 100);
            arParams[34].Value = newEmail;

            arParams[35] = new NpgsqlParameter("emailchangeguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[35].Value = emailChangeGuid.ToString();

            arParams[36] = new NpgsqlParameter("passwordresetguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[36].Value = passwordResetGuid.ToString();

            arParams[37] = new NpgsqlParameter("passwordsalt", NpgsqlTypes.NpgsqlDbType.Varchar, 128);
            arParams[37].Value = passwordSalt;

            arParams[38] = new NpgsqlParameter("roleschanged", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[38].Value = rolesChanged;

            arParams[39] = new NpgsqlParameter("authorbio", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[39].Value = authorBio;

            arParams[40] = new NpgsqlParameter("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp);
            if (dateOfBirth == DateTime.MinValue)
            {
                arParams[40].Value = DBNull.Value;
            }
            else
            {
                arParams[40].Value = dateOfBirth;
            }

            arParams[41] = new NpgsqlParameter("emailconfirmed", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[41].Value = emailConfirmed;

            arParams[42] = new NpgsqlParameter("pwdformat", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[42].Value = pwdFormat;

            arParams[43] = new NpgsqlParameter("passwordhash", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[43].Value = passwordHash;

            arParams[44] = new NpgsqlParameter("securitystamp", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[44].Value = securityStamp;

            arParams[45] = new NpgsqlParameter("phonenumber", NpgsqlTypes.NpgsqlDbType.Varchar, 50);
            arParams[45].Value = phoneNumber;

            arParams[46] = new NpgsqlParameter("phonenumberconfirmed", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[46].Value = phoneNumberConfirmed;

            arParams[47] = new NpgsqlParameter("twofactorenabled", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[47].Value = twoFactorEnabled;

            arParams[48] = new NpgsqlParameter("lockoutenddateutc", NpgsqlTypes.NpgsqlDbType.Timestamp);
            if (lockoutEndDateUtc == null)
            {
                arParams[48].Value = DBNull.Value;
            }
            else
            {
                arParams[48].Value = lockoutEndDateUtc;
            }


            int rowsAffected = await AdoHelper.ExecuteNonQueryAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return (rowsAffected > -1);

        }

        //public bool UpdatePasswordAndSalt(
        //    int userId,
        //    int pwdFormat,
        //    string password,
        //    string passwordSalt)
        //{
        //    StringBuilder sqlCommand = new StringBuilder();
        //    sqlCommand.Append("UPDATE mp_users ");
        //    sqlCommand.Append("SET    ");

        //    sqlCommand.Append("pwd = :password,    ");
        //    sqlCommand.Append("passwordsalt = :passwordsalt,    ");
        //    sqlCommand.Append("pwdformat = :pwdformat    ");

        //    sqlCommand.Append("WHERE userid = :userid ;");

        //    NpgsqlParameter[] arParams = new NpgsqlParameter[4];

        //    arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[0].Value = userId;

        //    arParams[1] = new NpgsqlParameter("password", NpgsqlTypes.NpgsqlDbType.Text);
        //    arParams[1].Value = password;

        //    arParams[2] = new NpgsqlParameter("passwordsalt", NpgsqlTypes.NpgsqlDbType.Varchar, 128);
        //    arParams[2].Value = passwordSalt;

        //    arParams[3] = new NpgsqlParameter("pwdformat", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[3].Value = pwdFormat;

        //    int rowsAffected = AdoHelper.ExecuteNonQuery(
        //        writeConnectionString,
        //        CommandType.Text,
        //        sqlCommand.ToString(),
        //        arParams);

        //    return (rowsAffected > -1);

        //}


        public async Task<bool> DeleteUser(int userId)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = userId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("DELETE FROM mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userid = :userid ");
            sqlCommand.Append(";");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_delete(:userid)",
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }

        public bool UpdateLastActivityTime(Guid userGuid, DateTime lastUpdate)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("lastactivitydate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = lastUpdate;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("set lastactivitydate = :lastactivitydate ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append(";");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_updatelastactivitytime(:userguid,:lastactivitydate)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public bool UpdateLastLoginTime(Guid userGuid, DateTime lastLoginTime)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");

            sqlCommand.Append("SET lastlogindate = :lastlogindate,  ");
            sqlCommand.Append("failedpasswordattemptcount = 0, ");
            sqlCommand.Append("failedpwdanswerattemptcount = 0 ");

            sqlCommand.Append("WHERE userguid = :userguid  ;");

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("lastlogindate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = lastLoginTime;

            int rowsAffected = AdoHelper.ExecuteNonQuery(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return (rowsAffected > -1);

        }

        public async Task<bool> AccountLockout(Guid userGuid, DateTime lockoutTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("lastlockoutdate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = lockoutTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET islockedout = true,  ");
            sqlCommand.Append("lastlockoutdate = :lastlockoutdate ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_accountlockout(:userguid,:lastlockoutdate)",
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }

        public bool UpdateLastPasswordChangeTime(Guid userGuid, DateTime lastPasswordChangeTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("lastpasswordchangeddate", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = lastPasswordChangeTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("lastpasswordchangeddate = :lastpasswordchangeddate ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_updatelastpasswordchangedate(:userguid,:lastpasswordchangeddate)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public bool UpdateFailedPasswordAttemptStartWindow(
            Guid userGuid,
            DateTime windowStartTime)
        {

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("windowstarttime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = windowStartTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("failedpwdattemptwindowstart = :windowstarttime ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_setfailedpasswordattemptstartwindow(:userguid,:windowstarttime)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public async Task<bool> UpdateFailedPasswordAttemptCount(
            Guid userGuid,
            int attemptCount)
        {

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("attemptcount", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = attemptCount;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("failedpasswordattemptcount = :attemptcount ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_setfailedpasswordattemptcount(:userguid,:attemptcount)",
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }

        public bool UpdateFailedPasswordAnswerAttemptStartWindow(
            Guid userGuid,
            DateTime windowStartTime)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("windowstarttime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            arParams[1].Value = windowStartTime;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("failedpwdanswerwindowstart = :windowstarttime ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_setfailedpasswordanswerattemptstartwindow(:userguid,:windowstarttime)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public bool UpdateFailedPasswordAnswerAttemptCount(
            Guid userGuid,
            int attemptCount)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("attemptcount", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = attemptCount;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("failedpwdanswerattemptcount = :attemptcount ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_setfailedpasswordanswerattemptcount(:userguid,:attemptcount)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public bool SetRegistrationConfirmationGuid(Guid userGuid, Guid registrationConfirmationGuid)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("registerconfirmguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[1].Value = registrationConfirmationGuid.ToString();

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("islockedout = true, ");
            sqlCommand.Append("registerconfirmguid = :registerconfirmguid ");
            sqlCommand.Append("WHERE userguid = :userguid  ;");

            //int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_setregistrationguid(:userguid,:registerconfirmguid)",
            //    arParams));

            int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams));

            return (rowsAffected > -1);

        }

        public async Task<bool> ConfirmRegistration(Guid emptyGuid, Guid registrationConfirmationGuid)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("emptyguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = emptyGuid.ToString();

            arParams[1] = new NpgsqlParameter("registerconfirmguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[1].Value = registrationConfirmationGuid.ToString();

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("islockedout = false, ");
            sqlCommand.Append("registerconfirmguid = :emptyguid ");
            sqlCommand.Append("WHERE registerconfirmguid = :registerconfirmguid  ;");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_confirmregistration(:emptyguid,:registerconfirmguid)",
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }

        public async Task<bool> AccountClearLockout(Guid userGuid)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET islockedout = false,  ");
            sqlCommand.Append("failedpasswordattemptcount = 0, ");
            sqlCommand.Append("failedpwdanswerattemptcount = 0 ");

            sqlCommand.Append("WHERE userguid = :userguid  ;");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }



        //public bool UpdatePasswordQuestionAndAnswer(
        //    Guid userGuid,
        //    String passwordQuestion,
        //    String passwordAnswer)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[3];

        //    arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
        //    arParams[0].Value = userGuid.ToString();

        //    arParams[1] = new NpgsqlParameter("passwordquestion", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
        //    arParams[1].Value = passwordQuestion;

        //    arParams[2] = new NpgsqlParameter("passwordanswer", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
        //    arParams[2].Value = passwordAnswer;

        //    int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
        //        writeConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_updatepasswordquestionandanswer(:userguid,:passwordquestion,:passwordanswer)",
        //        arParams));

        //    return (rowsAffected > -1);

        //}

        //public async Task<bool> UpdateTotalRevenue(Guid userGuid)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[1];

        //    arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
        //    arParams[0].Value = userGuid.ToString();

        //    int rowsAffected = await AdoHelper.ExecuteNonQueryAsync(
        //        writeConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_updatetotalarevenuebyuser(:userguid)",
        //        arParams);

        //    return rowsAffected > 0;
        //}

        //public async Task<bool> UpdateTotalRevenue()
        //{
        //    int rowsAfffected = await AdoHelper.ExecuteNonQueryAsync(
        //        writeConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_updatetotalarevenue()",
        //        null);

        //    return rowsAfffected > 0;
        //}


        public async Task<bool> FlagAsDeleted(int userId)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = userId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("isdeleted = true ");
            
            sqlCommand.Append("WHERE userid = :userid  ;");

            //object result = await AdoHelper.ExecuteScalarAsync(
            //    writeConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_flagasdeleted(:userid)",
            //    arParams);

            object result = await AdoHelper.ExecuteScalarAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int rowsAffected = Convert.ToInt32(result);

            return (rowsAffected > -1);

        }

        public async Task<bool> FlagAsNotDeleted(int userId)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_users ");
            sqlCommand.Append("SET isdeleted = false  ");

            sqlCommand.Append("WHERE userid = :userid  ;");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = userId;

            int rowsAffected = await AdoHelper.ExecuteNonQueryAsync(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return (rowsAffected > -1);
        }

        //public bool IncrementTotalPosts(int userId)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[1];

        //    arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[0].Value = userId;

        //    int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
        //        writeConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_incrementtotalposts(:userid)",
        //        arParams));

        //    return (rowsAffected > -1);

        //}

        //public bool DecrementTotalPosts(int userId)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[1];

        //    arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[0].Value = userId;

        //    int rowsAffected = Convert.ToInt32(AdoHelper.ExecuteScalar(
        //        writeConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_decrementtotalposts(:userid)",
        //        arParams));

        //    return (rowsAffected > -1);

        //}

        public async Task<DbDataReader> GetRolesByUser(int siteId, int userId)
        {

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT ");
            sqlCommand.Append("mp_roles.roleid, ");
            sqlCommand.Append("mp_roles.displayname, ");
            sqlCommand.Append("mp_roles.rolename ");

            sqlCommand.Append("FROM	 mp_userroles ");

            sqlCommand.Append("INNER JOIN mp_users ");
            sqlCommand.Append("ON mp_userroles.userid = mp_users.userid ");

            sqlCommand.Append("INNER JOIN mp_roles ");
            sqlCommand.Append("ON  mp_userroles.roleid = mp_roles.roleid ");

            sqlCommand.Append("WHERE mp_users.siteid = :siteid AND mp_users.userid = :userid  ;");

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[1].Value = userId;

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<DbDataReader> GetUserByRegistrationGuid(int siteId, Guid registerConfirmGuid)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("siteid = :siteid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("registerconfirmguid = :registerconfirmguid ");
            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("registerconfirmguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[1].Value = registerConfirmGuid;

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }


        public async Task<DbDataReader> GetSingleUser(int siteId, string email)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[1].Value = email.ToLower();

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE siteid = :siteid  ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("loweredemail = :email ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<DbDataReader> GetCrossSiteUserListByEmail(string email)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[0].Value = email.ToLower();

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            //sqlCommand.Append(" ");
            sqlCommand.Append("loweredemail = :email ");

            sqlCommand.Append(";");

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);



        }

        public async Task<DbDataReader> GetSingleUserByLoginName(int siteId, string loginName, bool allowEmailFallback)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT * ");

            sqlCommand.Append("FROM	mp_users ");

            sqlCommand.Append("WHERE siteid = :siteid  ");

            if (allowEmailFallback)
            {
                sqlCommand.Append("AND ");
                sqlCommand.Append("(");
                sqlCommand.Append("loginname = :loginname ");
                sqlCommand.Append("OR email = :loginname ");
                sqlCommand.Append(")");
            }
            else
            {
                sqlCommand.Append("AND loginname = :loginname ");
            }

            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("loginname", NpgsqlTypes.NpgsqlDbType.Text, 50);
            arParams[1].Value = loginName;

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public DbDataReader GetSingleUserByLoginNameNonAsync(int siteId, string loginName, bool allowEmailFallback)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT * ");

            sqlCommand.Append("FROM	mp_users ");

            sqlCommand.Append("WHERE siteid = :siteid  ");

            if (allowEmailFallback)
            {
                sqlCommand.Append("AND ");
                sqlCommand.Append("(");
                sqlCommand.Append("loginname = :loginname ");
                sqlCommand.Append("OR email = :loginname ");
                sqlCommand.Append(")");
            }
            else
            {
                sqlCommand.Append("AND loginname = :loginname ");
            }

            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("loginname", NpgsqlTypes.NpgsqlDbType.Text, 50);
            arParams[1].Value = loginName;

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<DbDataReader> GetSingleUser(int userId)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = userId;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userid = :userid ");
            sqlCommand.Append(";");

            //return await AdoHelper.ExecuteReaderAsync(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_selectone(:userid)",
            //    arParams);

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        public async Task<DbDataReader> GetSingleUser(Guid userGuid)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append(";");

            //return await AdoHelper.ExecuteReaderAsync(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_users_selectonebyguid(:userguid)",
            //    arParams);

            return await AdoHelper.ExecuteReaderAsync(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }

        //public Guid GetUserGuidFromOpenId(
        //    int siteId,
        //    string openIdUri)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[2];

        //    arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[0].Value = siteId;

        //    arParams[1] = new NpgsqlParameter("openiduri", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
        //    arParams[1].Value = openIdUri;

        //    Guid userGuid = Guid.Empty;


        //    using (DbDataReader reader = AdoHelper.ExecuteReader(
        //        readConnectionString,
        //        CommandType.StoredProcedure,
        //        "mp_users_selectguidbyopeniduri(:siteid,:openiduri)",
        //        arParams))
        //    {
        //        if (reader.Read())
        //        {
        //            userGuid = new Guid(reader["UserGuid"].ToString());
        //        }
        //    }

        //    return userGuid;

        //}

        //public Guid GetUserGuidFromWindowsLiveId(
        //    int siteId,
        //    string windowsLiveId)
        //{
        //    StringBuilder sqlCommand = new StringBuilder();
        //    sqlCommand.Append("SELECT  userguid ");
        //    sqlCommand.Append("FROM	mp_users ");
        //    sqlCommand.Append("WHERE ");
        //    sqlCommand.Append(" siteid = :siteid  ");
        //    sqlCommand.Append("AND windowsliveid = :windowsliveid ;  ");
        //    sqlCommand.Append(";");

        //    NpgsqlParameter[] arParams = new NpgsqlParameter[2];

        //    arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
        //    arParams[0].Value = siteId;

        //    arParams[1] = new NpgsqlParameter("windowsliveid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
        //    arParams[1].Value = windowsLiveId;

        //    Guid userGuid = Guid.Empty;

        //    using (DbDataReader reader = AdoHelper.ExecuteReader(
        //        readConnectionString,
        //        CommandType.Text,
        //        sqlCommand.ToString(),
        //        arParams))
        //    {
        //        if (reader.Read())
        //        {
        //            userGuid = new Guid(reader["UserGuid"].ToString());
        //        }
        //    }

        //    return userGuid;

        //}

        public string LoginByEmail(int siteId, string email, string password)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  name ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("email = :email  ");
            sqlCommand.Append("AND siteid = :siteid  ");
            sqlCommand.Append("AND isdeleted = false ");
            sqlCommand.Append("AND pwd = :password   ");
            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[3];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text, 100);
            arParams[1].Value = email;

            arParams[2] = new NpgsqlParameter("password", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[2].Value = password;

            string userName = string.Empty;

            using (DbDataReader reader = AdoHelper.ExecuteReader(
               readConnectionString,
               CommandType.Text,
               sqlCommand.ToString(),
               arParams))
            {
                if (reader.Read())
                {
                    userName = reader["Name"].ToString();
                }

            }

            return userName;

        }

        public string Login(int siteId, string loginName, string password)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  name ");
            sqlCommand.Append("FROM	mp_users ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("loginname = :loginname  ");
            sqlCommand.Append("AND siteid = :siteid  ");
            sqlCommand.Append("AND isdeleted = false ");
            sqlCommand.Append("AND pwd = :password  ");
            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[3];

            arParams[0] = new NpgsqlParameter("siteid", NpgsqlTypes.NpgsqlDbType.Integer);
            arParams[0].Value = siteId;

            arParams[1] = new NpgsqlParameter("loginname", NpgsqlTypes.NpgsqlDbType.Text, 50);
            arParams[1].Value = loginName;

            arParams[2] = new NpgsqlParameter("password", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[2].Value = password;

            string userName = string.Empty;

            using (DbDataReader reader = AdoHelper.ExecuteReader(
               readConnectionString,
               CommandType.Text,
               sqlCommand.ToString(),
               arParams))
            {
                if (reader.Read())
                {
                    userName = reader["Name"].ToString();
                }

            }

            return userName;

        }


        //public static DataTable GetNonLazyLoadedPropertiesForUser(Guid userGuid)
        //{
        //    NpgsqlParameter[] arParams = new NpgsqlParameter[1];

        //    arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
        //    arParams[0].Value = userGuid.ToString();

        //    DataTable dataTable = new DataTable();
        //    dataTable.Columns.Add("UserGuid", typeof(String));
        //    dataTable.Columns.Add("PropertyName", typeof(String));
        //    dataTable.Columns.Add("PropertyValueString", typeof(String));
        //    dataTable.Columns.Add("PropertyValueBinary", typeof(object));

        //    using (IDataReader reader = AdoHelper.ExecuteReader(
        //        ConnectionString.GetReadConnectionString(),
        //        CommandType.StoredProcedure,
        //        "mp_userproperties_select_byuser(:userguid)",
        //        arParams))
        //    {
        //        while (reader.Read())
        //        {
        //            DataRow row = dataTable.NewRow();
        //            row["UserGuid"] = reader["UserGuid"].ToString();
        //            row["PropertyName"] = reader["PropertyName"].ToString();
        //            row["PropertyValueString"] = reader["PropertyValueString"].ToString();
        //            row["PropertyValueBinary"] = reader["PropertyValueBinary"];
        //            dataTable.Rows.Add(row);
        //        }

        //    }

        //    return dataTable;

        //}

        public DbDataReader GetLazyLoadedProperty(Guid userGuid, String propertyName)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("propertyname", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[1].Value = propertyName;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  * ");
            sqlCommand.Append("FROM	mp_userproperties ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("propertyname = :propertyname ");
            sqlCommand.Append("LIMIT 1 ");
            sqlCommand.Append(";");

            //return AdoHelper.ExecuteReader(
            //    readConnectionString,
            //    CommandType.StoredProcedure,
            //    "mp_userproperties_select_one(:userguid,:propertyname",
            //    arParams);

            return AdoHelper.ExecuteReader(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

        }


        public bool PropertyExists(Guid userGuid, string propertyName)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[2];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("propertyname", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[1].Value = propertyName;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("SELECT  Count(*) ");
            sqlCommand.Append("FROM	mp_userproperties ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("propertyname = :propertyname ");
            sqlCommand.Append(";");

            //int count = Convert.ToInt32(AdoHelper.ExecuteScalar(
            //    readConnectionString,
            //        CommandType.StoredProcedure,
            //        "mp_userproperties_propertyexists(:userguid,:propertyname)",
            //        arParams));

            object obj = AdoHelper.ExecuteScalar(
                readConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            int count = Convert.ToInt32(obj);

            return (count > 0);
        }

        public void CreateProperty(
            Guid propertyId,
            Guid userGuid,
            String propertyName,
            String propertyValues,
            byte[] propertyValueb,
            DateTime lastUpdatedDate,
            bool isLazyLoaded)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[7];

            arParams[0] = new NpgsqlParameter("propertyid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = propertyId.ToString();

            arParams[1] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[1].Value = userGuid.ToString();

            arParams[2] = new NpgsqlParameter("propertyname", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[2].Value = propertyName;

            arParams[3] = new NpgsqlParameter("propertyvaluestring", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[3].Value = propertyValues;

            arParams[4] = new NpgsqlParameter("propertyvaluebinary", NpgsqlTypes.NpgsqlDbType.Bytea);
            arParams[4].Value = propertyValueb;

            arParams[5] = new NpgsqlParameter("lastupdateddate", NpgsqlTypes.NpgsqlDbType.Date);
            arParams[5].Value = lastUpdatedDate;

            arParams[6] = new NpgsqlParameter("islazyloaded", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[6].Value = isLazyLoaded;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("INSERT INTO mp_userproperties (");
            sqlCommand.Append("propertyid, ");
            sqlCommand.Append("userguid, ");
            sqlCommand.Append("propertyname, ");
            sqlCommand.Append("propertyvaluestring, ");
            sqlCommand.Append("propertyvaluebinary, ");
            sqlCommand.Append("lastupdateddate, ");
            sqlCommand.Append("islazyloaded )");

            sqlCommand.Append(" VALUES (");
            sqlCommand.Append(":propertyid, ");
            sqlCommand.Append(":userguid, ");
            sqlCommand.Append(":propertyname, ");
            sqlCommand.Append(":propertyvaluestring, ");
            sqlCommand.Append(":propertyvaluebinary, ");
            sqlCommand.Append(":lastupdateddate, ");
            sqlCommand.Append(":islazyloaded ");
            sqlCommand.Append(")");
            sqlCommand.Append(";");

            //AdoHelper.ExecuteNonQuery(
            //    writeConnectionString,
            //        CommandType.StoredProcedure,
            //        "mp_userproperties_insert(:propertyid,:userguid,:propertyname,:propertyvaluestring,:propertyvaluebinary,:lastupdateddate,:islazyloaded)",
            //        arParams);

            AdoHelper.ExecuteNonQuery(
                writeConnectionString,
                    CommandType.Text,
                    sqlCommand.ToString(),
                    arParams);


        }

        public void UpdateProperty(
            Guid userGuid,
            String propertyName,
            String propertyValues,
            byte[] propertyValueb,
            DateTime lastUpdatedDate,
            bool isLazyLoaded)
        {
            NpgsqlParameter[] arParams = new NpgsqlParameter[6];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Varchar, 36);
            arParams[0].Value = userGuid.ToString();

            arParams[1] = new NpgsqlParameter("propertyname", NpgsqlTypes.NpgsqlDbType.Varchar, 255);
            arParams[1].Value = propertyName;

            arParams[2] = new NpgsqlParameter("propertyvaluestring", NpgsqlTypes.NpgsqlDbType.Text);
            arParams[2].Value = propertyValues;

            arParams[3] = new NpgsqlParameter("propertyvaluebinary", NpgsqlTypes.NpgsqlDbType.Bytea);
            arParams[3].Value = propertyValueb;

            arParams[4] = new NpgsqlParameter("lastupdateddate", NpgsqlTypes.NpgsqlDbType.Date);
            arParams[4].Value = lastUpdatedDate;

            arParams[5] = new NpgsqlParameter("islazyloaded", NpgsqlTypes.NpgsqlDbType.Boolean);
            arParams[5].Value = isLazyLoaded;

            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("UPDATE mp_userproperties ");
            sqlCommand.Append("SET ");
            sqlCommand.Append("propertyvaluestring = :propertyvaluestring, ");
            sqlCommand.Append("propertyvaluebinary = :propertyvaluebinary, ");
            sqlCommand.Append("lastupdateddate = :lastupdateddate, ");
            sqlCommand.Append("islazyloaded = :islazyloaded ");

            sqlCommand.Append("WHERE  ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append("AND ");
            sqlCommand.Append("propertyname = :propertyname ");
            sqlCommand.Append(";");

            //AdoHelper.ExecuteNonQuery(
            //    writeConnectionString,
            //        CommandType.StoredProcedure,
            //        "mp_userproperties_update(:userguid,:propertyname,:propertyvaluestring,:propertyvaluebinary,:lastupdateddate,:islazyloaded)",
            //        arParams);

            AdoHelper.ExecuteNonQuery(
                writeConnectionString,
                    CommandType.Text,
                    sqlCommand.ToString(),
                    arParams);


        }


        public bool DeletePropertiesByUser(Guid userGuid)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("DELETE FROM mp_userproperties ");
            sqlCommand.Append("WHERE ");
            sqlCommand.Append("userguid = :userguid ");
            sqlCommand.Append(";");

            NpgsqlParameter[] arParams = new NpgsqlParameter[1];

            arParams[0] = new NpgsqlParameter("userguid", NpgsqlTypes.NpgsqlDbType.Char, 36);
            arParams[0].Value = userGuid.ToString();

            int rowsAffected = AdoHelper.ExecuteNonQuery(
                writeConnectionString,
                CommandType.Text,
                sqlCommand.ToString(),
                arParams);

            return (rowsAffected > -1);

        }



    }
}
