import { Routes } from '@angular/router';
import { SearchComponent } from './features/pages/ViewUser/search/search.component';
import { LoginComponent } from './features/pages/login/login.component';
import { HomeComponent } from './features/pages/home/home.component';
import { RegisterComponent } from './features/pages/ViewUser/register/register.component';
import { ProfileComponent } from './features/pages/profile/profile.component';
import { AdminUserComponent } from './features/pages/ViewAdmin/ManageUsers/admin-user.component';
import { AdminVideoComponent } from './features/pages/ViewAdmin/ManageVideos/admin-video.component';
import { LearningAdminComponent } from './features/pages/ViewAdmin/ManageLessons/admin-lessons.component';
import { UserLearningComponent } from './features/pages/ViewUser/courses/user-lessons.component';
export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'profile', component: ProfileComponent },

  { path: 'admin/users', component: AdminUserComponent },
  { path: 'admin/videos', component: AdminVideoComponent },
  { path: 'admin/lessons', component: LearningAdminComponent },

  { path: 'user/lessons', component: UserLearningComponent },
  { path: 'user/search', component: SearchComponent },
  { path: 'user/register', component: RegisterComponent },
];
