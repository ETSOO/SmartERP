using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person service
    /// 人员服务
    /// </summary>
    public class PersonService : SEUserService, IPersonService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Choose persons
        /// 选择人员
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ChoosePersonsData> ChoosePersonsAsync(ChoosePersonsRQ rq, CancellationToken cancellationToken = default)
        {
            // Max items
            var maxItems = rq.MaxItems > 0 ? rq.MaxItems : 20;

            // Users
            var users = await _db.Users(User.OrganizationInt).AsNoTracking()
                .OrderByDescending(p => p.RefreshTime)
                .Take(maxItems)
                .Select(p => new PersonListItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    JobTitle = p.JobTitle
                })
                .ToListAsync(cancellationToken);

            // Contacts
            var contacts = await _db.PersonRelations.AsNoTracking()
                .Where(r => r.PersonId == rq.PersonId && r.Person.OrgId == User.OrganizationInt)
                .OrderByDescending(r => r.Contact.RefreshTime)
                .Take(maxItems)
                .Select(r => new PersonListItem
                {
                    Id = r.Contact.Id,
                    Name = r.Contact.Name,
                    JobTitle = r.Contact.JobTitle
                })
                .ToListAsync(cancellationToken);

            // Return
            return new ChoosePersonsData
            {
                Users = users,
                Contacts = contacts
            };
        }

        /// <summary>
        /// Create address
        /// 创建地址
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAddressAsync(AddressCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new Person
               {
                   Id = p.Id,
                   IdentityType = p.IdentityType
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Find possible existing address
            var addressId = await _db.PersonAddresses.AsNoTracking()
                .Where(a => a.PersonId == person.Id && a.City == rq.City && a.FormattedAddress == rq.FormattedAddress)
                .Select(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (addressId < 1)
            {
                // New address
                var addr = new PersonAddress
                {
                    PersonId = person.Id,
                    Kind = rq.Kind,
                    Name = rq.Name,
                    PlaceId = rq.PlaceId,
                    Region = rq.Region,
                    State = rq.State,
                    City = rq.City,
                    District = rq.District,
                    Route = rq.Route,
                    Street = rq.Street,
                    PostalCode = rq.PostalCode,
                    FormattedAddress = rq.FormattedAddress,
                    Location = rq.Location == null ? null : new NpgsqlPoint(rq.Location.Lng, rq.Location.Lat),
                    Provider = rq.Provider
                };

                // Add
                _db.PersonAddresses.Add(addr);

                // Save
                await _db.SaveChangesAsync(cancellationToken);

                // Get the id
                addressId = addr.Id;
            }

            // Return
            return ActionResult.Succeed(addressId);
        }

        /// <summary>
        /// Create contact
        /// 创建联系人
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateContactAsync(ContactCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .AsNoTracking()
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new
               {
                   p.Id,
                   p.IdentityType
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.AddContact), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Is contact exists with same name
            var contactExists = await _db.PersonRelations.AsNoTracking()
                .AnyAsync(r => r.PersonId == rq.PersonId && r.Contact.Name == rq.Name, cancellationToken);

            if (contactExists)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Name));
            }

            // Create new contact
            var contact = new Person
            {
                OrgId = orgId,
                IdentityType = IdentityTypeFlags.Contact,
                Name = rq.Name,
                FamilyName = rq.FamilyName,
                GivenName = rq.GivenName,
                JobTitle = rq.JobTitle,
                Description = rq.Description,
                Title = rq.Title,
                Gender = rq.Gender,
                Birthday = rq.Birthday,
                CategoryIds = rq.Categories?.ToList(),
                Regions = rq.Regions?.ToList(),
                Cultures = rq.Cultures?.ToList()
            };

            if (!string.IsNullOrEmpty(rq.FamilyName))
            {
                contact.LatinFamilyName = ChineseUtils.GetPinyin(rq.FamilyName, true).ToPinyin();
            }

            if (!string.IsNullOrEmpty(rq.GivenName))
            {
                contact.LatinGivenName = ChineseUtils.GetPinyin(rq.GivenName, true).ToPinyin();
            }

            if (rq.Tags?.Any() is true)
            {
                var tagKind = _commonService.GetTagKind(IdentityTypeFlags.Contact);
                var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                contact.Tags = [.. tagIds];
            }

            _db.Persons.Add(contact);

            await _db.SaveChangesAsync(cancellationToken);

            // Add relation
            var relation = new PersonRelation
            {
                PersonId = person.Id,
                ContactId = contact.Id,
                RelationType = rq.RelationType
            };

            _db.PersonRelations.Add(relation);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(contact.Id);
        }

        /// <summary>
        /// Create info
        /// 创建信息
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateInfoAsync(PersonInfoCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new Person
               {
                   Id = p.Id,
                   IdentityType = p.IdentityType,
                   Infos = p.Infos
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.Persons.Attach(person);

            // Create infos
            foreach (var item in rq.Items)
            {
                // Check if the info already exists
                if (person.Infos.Any(i => i.Kind == item.Kind && i.Identifier == item.Identifier))
                {
                    continue;
                }

                // Create new info
                var info = new PersonInfo
                {
                    PersonId = person.Id,
                    Kind = item.Kind,
                    Identifier = item.Identifier,
                    Description = item.Description,
                    IsDefault = item.IsDefault ?? false
                };

                person.Infos.Add(info);
            }

            // Save changes
            var affected = await _db.SaveChangesAsync(cancellationToken);

            if (affected == 0)
            {
                return ApplicationErrors.ItemExists.AsResult();
            }

            // Return
            return ActionResult.Succeed(rq.PersonId);
        }

        private IQueryable<Person> CreateQuery(PersonListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        q = q.Where(p => (p.IdentityType & rq.IdentityType.Value) > 0);
                    }

                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.CategoryIds != null && rq.CategoryIds.Any(c => p.CategoryIds.Contains(c)));
                    }

                    if (rq.Education.HasValue)
                    {
                        q = q.Where(p => p.Education == rq.Education.Value);
                    }

                    if (!string.IsNullOrEmpty(rq.City))
                    {
                        q = q.Where(p => p.Addresses.Any(a => a.City == rq.City));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, p => p.Name, p => p.PreferredName);
                        }
                        else
                        {
                            q = q.Where(p => EF.Functions.ILike(p.Name, $"%{keyword}%")
                            || (p.QueryKeyword != null && EF.Functions.ILike(p.QueryKeyword, $"%{keyword}%"))
                            || (p.PreferredName != null && EF.Functions.ILike(p.PreferredName, $"%{keyword}%")));
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        private IQueryable<PersonRelation> CreateContactQuery(ContactListRQ rq, Func<IQueryable<PersonRelation>, IQueryable<PersonRelation>>? filters = null)
        {
            var query = _db.PersonRelations(User.OrganizationInt, rq.PersonId).AsNoTracking()
                .QueryEtsoo(rq, (q) => q.ContactId, (q) => q.Contact.Status, (q) =>
                {
                    if (rq.RelationType.HasValue)
                    {
                        q = q.Where(r => r.RelationType == rq.RelationType.Value);
                    }

                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Contact.Tags != null && p.Contact.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.Contact.CategoryIds != null && p.Contact.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.Contact.CategoryIds != null && rq.CategoryIds.Any(c => p.Contact.CategoryIds.Contains(c)));
                    }

                    if (!string.IsNullOrEmpty(rq.City))
                    {
                        q = q.Where(p => p.Contact.Addresses.Any(a => a.City == rq.City));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, p => p.Contact.Name, p => p.Contact.PreferredName);
                        }
                        else
                        {
                            q = q.Where(p => EF.Functions.ILike(p.Contact.Name, $"%{keyword}%")
                            || (p.Contact.QueryKeyword != null && EF.Functions.ILike(p.Contact.QueryKeyword, $"%{keyword}%"))
                            || (p.Contact.PreferredName != null && EF.Functions.ILike(p.Contact.PreferredName, $"%{keyword}%")));
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// Delete address
        /// 删除地址
        /// </summary>
        /// <param name="id">Address id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAddressAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var addr = await _db.PersonAddresses
               .Where(a => a.Id == id && a.Person.OrgId == orgId)
               .Select(a => new { a.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (addr == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(addr.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var result = await _db.PersonAddresses.AsNoTracking()
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else
            {
                return ActionResult.Succeed(id);
            }
        }

        /// <summary>
        /// Delete info
        /// 删除信息
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
               .Where(i => i.Id == id && i.Person.OrgId == orgId)
               .Select(i => new { i.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var result = await _db.PersonInfos.AsNoTracking()
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else
            {
                return ActionResult.Succeed(id);
            }
        }

        private void FormatListRQ(PersonListRQ rq)
        {
            if (rq.Id == 0)
            {
                rq.Id = User.Oid;
            }
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<ContactItem>> ListAsync(PersonListRQ rq, CancellationToken cancellationToken = default)
        {
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            rq.IdentityType = _commonService.MergeIdentityType(rq.IdentityType, identityType);
            if (rq.IdentityType == IdentityTypeFlags.None)
            {
                return [];
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq);

            return await query.Select(p => new ContactItem
            {
                Id = p.Id,
                Name = p.Name,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                {
                    Name = o.Person.Name,
                    IdentityType = o.Person.IdentityType
                }).FirstOrDefault() : null,
                JobTitle = p.JobTitle,
                PreferredName = p.PreferredName
            }).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(PersonListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgs = await ListAsync(rq, cancellationToken);
            await writer.SerializeAsync(orgs, PlatformSharedContext.Default.IEnumerableContactItem);
        }

        /// <summary>
        /// List contacts
        /// 联系人列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListContactAsync(ContactListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdatePersonTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateContactQuery(rq)
                .Select(c => new
                {
                    Id = c.ContactId,
                    c.RelationType,
                    c.Contact.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<PersonQueryData>> QueryAsync(PersonQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            rq.IdentityType = _commonService.MergeIdentityType(rq.IdentityType, identityType);
            if (rq.IdentityType == IdentityTypeFlags.None)
            {
                return [];
            }

            await _commonService.UpdatePersonTagAsync(rq, User.OrganizationInt, cancellationToken);

            FormatListRQ(rq);

            var query = CreateQuery(rq, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.AssignedId))
                {
                    q = q.Where(p => p.AssignedId != null && EF.Functions.ILike(p.AssignedId, $"{rq.AssignedId}%"));
                }

                if (!string.IsNullOrEmpty(rq.JobTitle))
                {
                    q = q.Where(p => p.JobTitle != null && EF.Functions.ILike(p.JobTitle, $"%{rq.JobTitle}%"));
                }

                if (!string.IsNullOrEmpty(rq.Description))
                {
                    q = q.Where(p => p.Description != null && EF.Functions.ILike(p.Description, $"%{rq.Description}%"));
                }

                if (!string.IsNullOrEmpty(rq.Info))
                {
                    q = q.Where(p => p.Infos.Any(i => i.Identifier == rq.Info));
                }

                if (!string.IsNullOrEmpty(rq.Address))
                {
                    q = q.Where(p => p.Addresses.Any(a => EF.Functions.ILike(a.FormattedAddress, $"%{rq.Address}%")));
                }

                return q;
            });

            return await query.Select(p => new PersonQueryData
            {
                Id = p.Id,
                Name = p.Name,
                PreferredName = p.PreferredName,
                AssignedId = p.AssignedId,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                {
                    Name = o.Person.Name,
                    IdentityType = o.Person.IdentityType
                }).FirstOrDefault() : null,
                JobTitle = p.JobTitle,
                Status = p.Status,
                Creation = p.Creation
            }).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(PersonQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var persons = await QueryAsync(rq, cancellationToken);
            await writer.SerializeAsync(persons, MyJsonSerializerContext.Default.IEnumerablePersonQueryData);
        }

        /// <summary>
        /// Query contact JSON data
        /// 查询联系人JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryContactAsync(ContactQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdatePersonTagAsync(rq, User.OrganizationInt, cancellationToken);

            var query = CreateContactQuery(rq, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.JobTitle))
                {
                    q = q.Where(r => r.Contact.JobTitle != null && EF.Functions.ILike(r.Contact.JobTitle, $"%{rq.JobTitle}%"));
                }

                if (rq.Description?.Length > 1)
                {
                    var description = rq.Description;

                    if (description.IsComplexQueryKeywords())
                    {
                        q = q.QueryEtsooKeywords(description, DbUtils.ILikeMethod, r => r.Description, r => r.Contact.Description);
                    }
                    else
                    {
                        q = q.Where(r => (r.Description != null && EF.Functions.ILike(r.Description, $"%{description}%"))
                        || (r.Contact.Description != null && EF.Functions.ILike(r.Contact.Description, $"%{description}%")));
                    }
                }

                if (!string.IsNullOrEmpty(rq.Info))
                {
                    q = q.Where(r => r.Contact.Infos.Any(i => i.Identifier == rq.Info));
                }

                if (!string.IsNullOrEmpty(rq.Address))
                {
                    q = q.Where(r => r.Contact.Addresses.Any(a => EF.Functions.ILike(a.FormattedAddress, $"%{rq.Address}%")));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(r => new ContactQueryData
            {
                Id = r.ContactId,
                RelationType = r.RelationType,
                Name = r.Contact.Name,
                Description = r.Description,
                Creation = r.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryContactAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Query person info JSON data
        /// 查询人员信息JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task QueryInfoAsync(PersonInfoQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.PersonInfos.Where(i => i.PersonId == rq.PersonId && i.Person.OrgId == orgId)
                .AsNoTracking()
                .QueryEtsoo(rq, (i) => i.Id, null, (q) =>
                {
                    if (rq.Identifier?.Length > 1)
                    {
                        q = q.Where(i => i.Identifier == rq.Identifier);
                    }

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(i => i.Kind == rq.Kind.Value);
                    }

                    if (rq.IsDefault.HasValue)
                    {
                        q = q.Where(i => i.IsDefault == rq.IsDefault);
                    }

                    if (rq.IsVerified.HasValue)
                    {
                        q = q.Where(i => i.IsVerified == rq.IsVerified);
                    }

                    if (rq.Subscribed.HasValue)
                    {
                        q = q.Where(i => i.Subscribed == rq.Subscribed);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, p => p.Description);
                    }

                    return q;
                })
                .Select(i => new
                {
                    i.Id,
                    i.Kind,
                    Identifier = MyDbFunctions.HideData(i.Identifier, '@'),
                    i.Description,
                    i.IsDefault,
                    i.IsVerified,
                    i.Subscribed,
                    i.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read person data for view
        /// 读取用于浏览的人员数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager;

            if (id == 0)
            {
                // Current user
                id = User.Oid;
            }

            var data = await _db.Persons.AsNoTracking()
                .Include(p => p.ContactOwners).ThenInclude(o => o.Person)
                .Where(p => p.OrgId == orgId && p.Id == id && (p.IdentityType & identityType) > 0)
                .Select(p => new PersonViewData
                {
                    Id = p.Id,
                    Uid = p.Uid,
                    IdentityType = p.IdentityType,
                    Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                    {
                        Name = o.Person.Name,
                        IdentityType = o.Person.IdentityType
                    }).FirstOrDefault() : null,
                    IsLegalPerson = p.IsLegalPerson,
                    Name = p.Name,
                    GivenName = p.GivenName,
                    FamilyName = p.FamilyName,
                    LatinGivenName = p.LatinGivenName,
                    LatinFamilyName = p.LatinFamilyName,
                    PreferredName = p.PreferredName,
                    Title = p.Title,
                    Description = p.Description,
                    Avatar = p.Avatar ?? (p.CoreUser == null ? (p.CoreOrganization == null ? null : p.CoreOrganization.Logo) : p.CoreUser.Avatar),
                    JobTitle = p.JobTitle,
                    AssignedId = p.AssignedId,
                    Categories = p.CategoryIds == null ? null : _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    Addresses = p.Addresses.Select(a => new AddressItem { Id = a.Id, Kind = a.Kind, Name = a.Name, FormattedAddress = a.FormattedAddress }),
                    ReportTo = p.ReportTo,
                    ReportToName = p.ReportToUser == null ? null : p.ReportToUser.Name,
                    Creation = p.Creation,
                    Status = p.Status,
                    QueryKeyword = p.QueryKeyword,
                    Regions = p.Regions,
                    Currencies = p.Currencies,
                    Cultures = p.Cultures,

                    // Groups
                    ContactOwners = p.ContactOwners.Select(o => new LongIdItem
                    {
                        Id = o.PersonId,
                        Title = o.Person.Name
                    }),

                    /** Private **/
                    PrivateData = isPrivate || p.CoreUserId == userId || p.ReportTo == userId || p.UserId == userId ? new PersonPrivateData
                    {
                        Gender = p.Gender,
                        Birthday = p.Birthday,
                        Ethnicity = p.Ethnicity,
                        Height = p.Height,
                        Weight = p.Weight,
                        MaritalStatus = p.MaritalStatus,
                        Education = p.Education,
                        Degree = p.Degree,
                        PoliticalStatus = p.PoliticalStatus,
                    } : null,

                    /** Extention **/
                    Data = p.Data,

                    /** User **/
                    UserRole = p.UserRole,
                    Expiry = p.Expiry,
                    InviterName = p.Inviter == null ? null : p.Inviter.Name,
                    RefreshTime = p.RefreshTime

                }).FirstOrDefaultAsync(cancellationToken);

            if (data != null)
            {
                data.Editable = await _commonService.HasIdentityPermissionAsync(data.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken);
            }

            return data;
        }

        /// <summary>
        /// Read person info for view
        /// 读取用于浏览的人员信息
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string?> ReadInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
               .Where(i => i.Id == id && i.Person.OrgId == orgId)
               .Select(i => new { i.Identifier, i.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return null;
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.IdentityType, nameof(Permissions.Customer.View), cancellationToken))
            {
                return null;
            }

            return info.Identifier;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == rq.Id && p.OrgId == orgId)
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager || person.CoreUserId == userId || person.ReportTo == userId || person.UserId == userId;

            if (rq.ReportTo.HasValue && !await _db.Users(orgId).AnyAsync(u => u.Id == rq.ReportTo.Value, cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ReportTo));
            }

            if (rq.IsModified(nameof(rq.IdentityType)) && rq.IdentityType.HasValue)
            {
                if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
                {
                    return ApplicationErrors.AccessDenied.AsResult(nameof(rq.IdentityType));
                }

                person.IdentityType = rq.IdentityType.Value;
            }

            if (rq.IsModified(nameof(rq.IsLegalPerson)) && rq.IsLegalPerson.HasValue)
            {
                person.IsLegalPerson = rq.IsLegalPerson.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                person.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.GivenName)))
            {
                person.GivenName = rq.GivenName;
            }

            if (rq.IsModified(nameof(rq.FamilyName)))
            {
                person.FamilyName = rq.FamilyName;
            }

            if (rq.IsModified(nameof(rq.LatinGivenName)))
            {
                person.LatinGivenName = rq.LatinGivenName;
            }

            if (rq.IsModified(nameof(rq.LatinFamilyName)))
            {
                person.LatinFamilyName = rq.LatinFamilyName;
            }

            if (rq.IsModified(nameof(rq.PreferredName)))
            {
                person.PreferredName = rq.PreferredName;
            }

            if (rq.IsModified(nameof(rq.JobTitle)))
            {
                person.JobTitle = rq.JobTitle;
            }

            if (rq.IsModified(nameof(rq.Title)))
            {
                person.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                person.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                person.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.QueryKeyword)))
            {
                person.AssignedId = rq.QueryKeyword?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                person.CategoryIds = rq.Categories?.ToList();
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagKind = _commonService.GetTagKind(person.IdentityType);
                    var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                    person.Tags = [.. tagIds];
                }
                else
                {
                    person.Tags = null;
                }
            }

            if (rq.IsModified(nameof(rq.ReportTo)))
            {
                person.ReportTo = rq.ReportTo;
            }

            if (rq.IsModified(nameof(rq.Regions)))
            {
                person.Regions = rq.Regions?.ToList();
            }

            if (rq.IsModified(nameof(rq.Currencies)))
            {
                person.Currencies = rq.Currencies?.ToList();
            }

            if (rq.IsModified(nameof(rq.Cultures)))
            {
                person.Cultures = rq.Cultures?.ToList();
            }

            if (rq.IsModified(nameof(rq.Expiry)))
            {
                person.Expiry = rq.Expiry?.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                person.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                person.Data = rq.Data;
            }

            if (isPrivate && rq.PrivateData != null)
            {
                // Private data
                // If the user is not allowed to modify private data, skip these fields

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Gender)))
                {
                    person.Gender = rq.PrivateData.Gender?.ToUpper();
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Birthday)))
                {
                    person.Birthday = rq.PrivateData.Birthday?.ToUniversalTime();
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Ethnicity)))
                {
                    person.Ethnicity = rq.PrivateData.Ethnicity;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Height)))
                {
                    person.Height = rq.PrivateData.Height;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Weight)))
                {
                    person.Weight = rq.PrivateData.Weight;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.MaritalStatus)))
                {
                    person.MaritalStatus = rq.PrivateData.MaritalStatus;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Education)))
                {
                    person.Education = rq.PrivateData.Education;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Degree)))
                {
                    person.Degree = rq.PrivateData.Degree;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.PoliticalStatus)))
                {
                    person.PoliticalStatus = rq.PrivateData.PoliticalStatus;
                }
            }

            // Changes
            // var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update address
        /// 更新地址
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAddressAsync(AddressUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var addr = await _db.PersonAddresses
                .Where(a => a.Id == rq.Id && a.Person.OrgId == orgId)
                .Include(a => a.Person)
                .Select(a => new PersonAddress
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    Kind = a.Kind,
                    Name = a.Name,
                    PlaceId = a.PlaceId,
                    Region = a.Region,
                    State = a.State,
                    City = a.City,
                    District = a.District,
                    Route = a.Route,
                    Street = a.Street,
                    PostalCode = a.PostalCode,
                    FormattedAddress = a.FormattedAddress,
                    Location = a.Location,
                    Provider = a.Provider,
                    Person = new Person
                    {
                        Id = a.Person.Id,
                        IdentityType = a.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (addr == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(addr.Person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.PersonAddresses.Attach(addr);

            if (rq.IsModified(nameof(rq.PersonId)) && rq.PersonId.HasValue && rq.PersonId != addr.PersonId)
            {
                // Check if the person exists
                var person = await _db.Persons
                    .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
                    .Select(p => new Person
                    {
                        Id = p.Id,
                        IdentityType = p.IdentityType
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (person == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
                }

                if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
                {
                    return ApplicationErrors.AccessDenied.AsResult(nameof(rq.PersonId));
                }

                addr.PersonId = person.Id;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                addr.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                addr.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.PlaceId)))
            {
                addr.PlaceId = rq.PlaceId;
            }

            if (rq.IsModified(nameof(rq.Region)) && !string.IsNullOrEmpty(rq.Region))
            {
                addr.Region = rq.Region;
            }

            if (rq.IsModified(nameof(rq.State)) && !string.IsNullOrEmpty(rq.State))
            {
                addr.State = rq.State;
            }

            if (rq.IsModified(nameof(rq.City)) && !string.IsNullOrEmpty(rq.City))
            {
                addr.City = rq.City;
            }

            if (rq.IsModified(nameof(rq.District)))
            {
                addr.District = rq.District;
            }

            if (rq.IsModified(nameof(rq.Route)))
            {
                addr.Route = rq.Route;
            }

            if (rq.IsModified(nameof(rq.Street)))
            {
                addr.Street = rq.Street;
            }

            if (rq.IsModified(nameof(rq.PostalCode)))
            {
                addr.PostalCode = rq.PostalCode;
            }

            if (rq.IsModified(nameof(rq.FormattedAddress)) && !string.IsNullOrEmpty(rq.FormattedAddress))
            {
                addr.FormattedAddress = rq.FormattedAddress;
            }

            if (rq.IsModified(nameof(rq.Location)))
            {
                addr.Location = rq.Location == null ? null : new NpgsqlPoint(rq.Location.Lng, rq.Location.Lat);
            }

            if (rq.IsModified(nameof(rq.Provider)) && rq.Provider.HasValue)
            {
                addr.Provider = rq.Provider.Value;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update contact relation
        /// 更新联系人关系
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateContactRelationAsync(ContactRelationUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var relation = await _db.PersonRelations
                .Where(r => r.PersonId == rq.PersonId && r.ContactId == rq.ContactId && r.Person.OrgId == orgId)
                .Include(r => r.Person)
                .Select(r => new PersonRelation
                {
                    RelationType = r.RelationType,
                    Description = r.Description,
                    Data = r.Data,
                    Person = new Person
                    {
                        Id = r.Person.Id,
                        IdentityType = r.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (relation == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(relation.Person.IdentityType, nameof(Permissions.Customer.AddContact), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.PersonRelations.Attach(relation);

            if (rq.IsModified(nameof(rq.RelationType)) && rq.RelationType.HasValue)
            {
                relation.RelationType = rq.RelationType.Value;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                relation.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                relation.Data = rq.Data;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.PersonId);
        }

        /// <summary>
        /// Update info
        /// 更新信息
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateInfoAsync(PersonInfoUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
                .Where(i => i.Id == rq.Id && i.Person.OrgId == orgId)
                .Include(i => i.Person)
                .Select(i => new PersonInfo
                {
                    Id = i.Id,
                    Kind = i.Kind,
                    Identifier = i.Identifier,
                    Description = i.Description,
                    IsDefault = i.IsDefault,
                    Subscribed = i.Subscribed,
                    Person = new Person
                    {
                        Id = i.Person.Id,
                        IdentityType = i.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.Person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.PersonInfos.Attach(info);

            if (rq.IsModified(nameof(rq.Kind)))
            {
                info.Kind = rq.Kind;
            }

            if (rq.IsModified(nameof(rq.Identifier)) && !string.IsNullOrEmpty(rq.Identifier))
            {
                info.Identifier = rq.Identifier;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                info.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.IsDefault)) && rq.IsDefault.HasValue)
            {
                info.IsDefault = rq.IsDefault.Value;
            }

            if (rq.IsModified(nameof(rq.Subscribed)))
            {
                info.Subscribed = rq.Subscribed;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read address data for update
        /// 读取用于更新地址的数据
        /// </summary>
        /// <param name="id">Address id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AddressUpdateReadData?> UpdateAddressReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return await _db.PersonAddresses.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AddressUpdateReadData
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    Kind = a.Kind,
                    Provider = a.Provider,
                    PlaceId = a.PlaceId,
                    Name = a.Name,
                    Region = a.Region,
                    State = a.State,
                    City = a.City,
                    District = a.District,
                    Route = a.Route,
                    Street = a.Street,
                    PostalCode = a.PostalCode,
                    FormattedAddress = a.FormattedAddress,
                    Location = a.Location == null ? null : new Location((float)a.Location.Value.X, (float)a.Location.Value.Y)
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read contact relation data for update
        /// 读取用于更新联系人关系的数据
        /// </summary>
        /// <param name="personId">Person id</param>
        /// <param name="contactId">Contact id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task UpdateContactRelationReadAsync(long personId, long contactId, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.PersonRelations.AsNoTracking()
                .Where(r => r.PersonId == personId && r.ContactId == contactId)
                .Select(r => new ContactRelationUpdateReadData
                {
                    RelationType = r.RelationType,
                    Description = r.Description,
                    Data = r.Data,
                    Creation = r.Creation
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager;

            return await _db.Persons.AsNoTracking()
                .Include(p => p.ContactOwners).ThenInclude(o => o.Person)
                .Where(p => p.OrgId == orgId && p.Id == id && (p.IdentityType & identityType) > 0)
                .Select(p => new PersonUpdateReadData
                {
                    Id = p.Id,
                    IdentityType = p.IdentityType,
                    IsLegalPerson = p.IsLegalPerson,
                    Name = p.Name,
                    GivenName = p.GivenName,
                    FamilyName = p.FamilyName,
                    LatinGivenName = p.LatinGivenName,
                    LatinFamilyName = p.LatinFamilyName,
                    PreferredName = p.PreferredName,
                    Title = p.Title,
                    Description = p.Description,
                    JobTitle = p.JobTitle,
                    AssignedId = p.AssignedId,
                    Categories = p.CategoryIds,
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    ReportTo = p.ReportTo,
                    QueryKeyword = p.QueryKeyword,
                    Regions = p.Regions,
                    Currencies = p.Currencies,
                    Cultures = p.Cultures,
                    Data = p.Data,
                    Expiry = p.Expiry,
                    Status = p.Status,

                    /** Private **/
                    PrivateData = isPrivate || p.CoreUserId == userId || p.ReportTo == userId || p.UserId == userId ? new PersonPrivateData
                    {
                        Gender = p.Gender,
                        Birthday = p.Birthday,
                        Ethnicity = p.Ethnicity,
                        Height = p.Height,
                        Weight = p.Weight,
                        MaritalStatus = p.MaritalStatus,
                        Education = p.Education,
                        Degree = p.Degree,
                        PoliticalStatus = p.PoliticalStatus,
                    } : null
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
