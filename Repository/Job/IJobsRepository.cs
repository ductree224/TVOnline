﻿using TVOnline.Models;

namespace TVOnline.Repository.Job
{
    public interface IJobsRepository
    {
        Users? FindUsersByEmail(string email);
    }
}
