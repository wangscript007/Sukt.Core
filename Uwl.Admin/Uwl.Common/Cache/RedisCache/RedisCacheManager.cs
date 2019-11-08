﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Uwl.Common.Helper;
using Uwl.Extends.Utility;

namespace Uwl.Common.Cache.RedisCache
{
    public class RedisCacheManager: IRedisCacheManager
    {
        private readonly string redisConnectionString;
        public volatile ConnectionMultiplexer redisConnection;
        private readonly object redisConnectionLock = new object();
        /// <summary>
        /// Redis管理器的构造函数
        /// </summary>
        public RedisCacheManager()
        {
            var _redisConnection = Appsettings.app(new string[] { "RedisCaching", "ConnectionString" });//获取Redis链接字符串
            if (string.IsNullOrWhiteSpace(_redisConnection))
            {
                throw new ArgumentException("redis config is empty", nameof(_redisConnection));
            }
            this.redisConnectionString = _redisConnection;
            this.redisConnection = GetRedisConnection();///获取Redis链接实例服务
        }
        /// <summary>
        /// 核心代码，获取链接方式
        /// 通过双If 加 lock的方式实现单例模式
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetRedisConnection()
        {
            //如果已存在链接实例，直接返回
            if (this.redisConnection != null && this.redisConnection.IsConnected)
            {
                return this.redisConnection;
            }
            //加锁，防止异步编程中，出现单例无效的问题
            lock (redisConnectionLock)
            {
                if (this.redisConnection != null)
                {
                    this.redisConnection.Dispose();//如果存在实例链接释放Redis链接
                }
                try
                {
                    this.redisConnection = ConnectionMultiplexer.Connect(this.redisConnectionString);
                }
                catch (Exception)
                {
                    throw new Exception("Redis服务未启用，请开启该服务");
                }
            }
            return this.redisConnection;
        }
        public void Clear()
        {
            foreach (var item in this.GetRedisConnection().GetEndPoints())
            {
                var server = this.GetRedisConnection().GetServer(item);
                foreach (var keys in server.Keys())
                {
                    redisConnection.GetDatabase().KeyDelete(keys);
                }
            }
        }

        public TEntity Get<TEntity>(string key)
        {
            var value = redisConnection.GetDatabase().StringGet(key);
            if (value.HasValue)
            {
                //需要用的反序列化，将Redis存储的Byte[]，进行反序列化
                return SerializeHelper.Deserialize<TEntity>(value);
            }
            else
            {
                return default(TEntity);
            }
        }
        public List<TEntity> GetList<TEntity>(string key)
        {
            if (key.IsNullOrEmpty())
                throw new Exception("要获取缓存的Key不可为空");
            var value = redisConnection.GetDatabase().StringGet(key);
            if(value.HasValue)
            {
                var list=SerializeHelper.Deserialize<List<TEntity>>(value);
                return list;
            }
            else
            {
                return new List<TEntity>();
            }
        }

        public bool Get(string key)
        {
            return redisConnection.GetDatabase().KeyExists(key);
        }

        public string GetValue(string key)
        {
            return redisConnection.GetDatabase().StringGet(key);
        }

        public void Remove(string key)
        {
            redisConnection.GetDatabase().KeyDelete(key);
        }

        public void Set(string key, object value, TimeSpan? cacheTime=null)
        {
            if (value != null)
            {
                if(cacheTime!=null)
                {
                    redisConnection.GetDatabase().StringSet(key, SerializeHelper.Serialize(value), cacheTime);
                }
                else
                {
                    redisConnection.GetDatabase().StringSet(key, SerializeHelper.Serialize(value));
                }
                
            }
        }
    }
}
