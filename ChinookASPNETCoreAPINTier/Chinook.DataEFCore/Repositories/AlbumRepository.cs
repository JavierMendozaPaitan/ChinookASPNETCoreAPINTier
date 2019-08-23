﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Chinook.Domain.Repositories;
using Chinook.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.DataEFCore.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly ChinookContext _context;
        
        private IMemoryCache _cache;

        public AlbumRepository(ChinookContext context, IMemoryCache memoryCache)
        {
            _context = context;
            
            _cache = memoryCache;
        }

        private async Task<bool> AlbumExists(int id, CancellationToken ct = default) =>
            await _context.Album.AnyAsync(a => a.AlbumId == id, ct);

        public void Dispose() => _context.Dispose();

        public async Task<List<Album>> GetAllAsync(CancellationToken ct = default) => await _context.Album.ToListAsync(ct);

        public async Task<Album> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var cachedAlbum = _cache.Get<Album>(id);

            if (cachedAlbum != null)
            {
                return cachedAlbum;
            }
            else
            {
                var dbAlbum = await _context.Album.FindAsync(id);

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800));
                _cache.Set(dbAlbum.AlbumId, dbAlbum, cacheEntryOptions);

                return dbAlbum;
            }
        }

        public async Task<Album> AddAsync(Album newAlbum, CancellationToken ct = default)
        {
            _context.Album.Add(newAlbum);
            await _context.SaveChangesAsync(ct);
            return newAlbum;
        }

        public async Task<bool> UpdateAsync(Album album, CancellationToken ct = default)
        {
            if (!await AlbumExists(album.AlbumId, ct))
                return false;
            _context.Album.Update(album);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (!await AlbumExists(id, ct))
                return false;
            var toRemove = _context.Album.Find(id);
            _context.Album.Remove(toRemove);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<Album>> GetByArtistIdAsync(int id, CancellationToken ct = default) => await _context.Album.Where(a => a.ArtistId == id).ToListAsync(ct);
    }
}