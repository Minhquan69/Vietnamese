import { Routes } from '@angular/router';
import { SearchComponent } from './features/pages/ViewUser/search/search.component';
import { LoginComponent } from './features/pages/login/login.component';
import { HomeComponent } from './features/pages/home/home.component';
import { RegisterComponent } from './features/pages/ViewUser/register/register.component';
import { ProfileComponent } from './features/pages/profile/profile.component';
import { AdminUserComponent } from './features/pages/ViewAdmin/ManageUsers/admin-user.component';
import { AdminVideoComponent } from './features/pages/ViewAdmin/ManageVideos/admin-video.component';
export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'search', component: SearchComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'admin/users', component: AdminUserComponent },
  { path: 'admin/videos', component: AdminVideoComponent },
];
