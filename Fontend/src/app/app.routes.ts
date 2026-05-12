import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard.page';
import { SearchComponent } from './features/pages/ViewUser/search/search.component';
import { LoginComponent } from './features/pages/login/login.component';
import { HomeComponent } from './features/pages/home/home.component';
import { RegisterComponent } from './features/pages/ViewUser/register/register.component';
import { ProfileComponent } from './features/pages/profile/profile.component';
import { AdminUserComponent } from './features/pages/ViewAdmin/ManageUsers/admin-user.component';
import { AdminVideoComponent } from './features/pages/ViewAdmin/ManageVideos/admin-video.component';
import { LevelComponent } from './features/pages/ViewAdmin/Learning/adminLevel/level.component';
import { CourseComponent } from './features/pages/ViewAdmin/Learning/adminCourse/course.component';
import { UnitComponent } from './features/pages/ViewAdmin/Learning/adminUnit/unit.component';
import { UnitDetailComponent } from './features/pages/ViewAdmin/Learning/adminUnitDetail/unit-detail.component';
import { QuizComponent } from './features/pages/ViewAdmin/Learning/quiz/test.component';
import { PlacementComponent } from './features/pages/ViewAdmin/ManageTests/placement.component';
import { MyProgressComponent } from './features/pages/ViewUser/Learning/unit/unit.component';
import { QuizLearnComponent } from './features/pages/ViewUser/Learning/quiz/quiz.component';
import { PlacementUserComponent } from './features/pages/ViewUser/practice/placement.component';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { ForgotPasswordComponent } from './features/pages/auth/forgot-password.component';
import { ResetPasswordComponent } from './features/pages/auth/reset-password.component';
import { LearnHomeComponent } from './pages/learn/learn-home.component';
import { CourseListComponent } from './pages/learn/course-list.component';
import { CourseDetailComponent } from './pages/learn/course-detail.component';
import { UnitShellComponent } from './pages/learn/unit-shell.component';
import { LessonPlayerComponent } from './pages/learn/lesson-player.component';
import { VocabularyHubComponent } from './pages/vocabulary/vocabulary-hub.component';
import { InteractiveQuizPlayerComponent } from './pages/quiz-interactive/interactive-quiz-player.component';
import { AiTutorPageComponent } from './pages/ai-tutor/ai-tutor.page';
import { SpeakingLabPageComponent } from './pages/speaking-lab/speaking-lab.page';
import { GamificationArenaPageComponent } from './pages/gamification-arena/gamification-arena.page';
import { AdminCmsLayoutComponent } from './pages/admin-cms/admin-cms-layout.component';
import { AdminCmsDashboardPageComponent } from './pages/admin-cms/admin-cms-dashboard.page';
import { AdminCmsUsersPageComponent } from './pages/admin-cms/admin-cms-users.page';
import { AdminCmsCoursesPageComponent } from './pages/admin-cms/admin-cms-courses.page';
import { AdminCmsLessonsPageComponent } from './pages/admin-cms/admin-cms-lessons.page';
import { AdminCmsVocabularyPageComponent } from './pages/admin-cms/admin-cms-vocabulary.page';
import { AdminCmsQuizzesPageComponent } from './pages/admin-cms/admin-cms-quizzes.page';

const adminModerator = { roles: ['Admin', 'Moderator'] };

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'user/register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardPageComponent, canActivate: [authGuard] },
      {
        path: 'learn',
        children: [
          { path: '', component: LearnHomeComponent },
          { path: 'level/:levelId', component: CourseListComponent },
          {
            path: 'course/:courseId',
            component: CourseDetailComponent,
            canActivate: [authGuard],
          },
          {
            path: 'course/:courseId/unit/:unitId',
            component: UnitShellComponent,
            canActivate: [authGuard],
            children: [{ path: 'lesson/:lessonId', component: LessonPlayerComponent }],
          },
        ],
      },
      { path: 'home', component: HomeComponent },
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [authGuard],
      },
      {
        path: 'admin/users',
        component: AdminUserComponent,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] },
      },
      {
        path: 'admin/cms',
        component: AdminCmsLayoutComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
        children: [
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
          { path: 'dashboard', component: AdminCmsDashboardPageComponent },
          { path: 'courses', component: AdminCmsCoursesPageComponent },
          { path: 'lessons', component: AdminCmsLessonsPageComponent },
          { path: 'vocabulary', component: AdminCmsVocabularyPageComponent },
          { path: 'quizzes', component: AdminCmsQuizzesPageComponent },
          {
            path: 'users',
            component: AdminCmsUsersPageComponent,
            canActivate: [roleGuard],
            data: { roles: ['Admin'] },
          },
        ],
      },
      {
        path: 'admin/videos',
        component: AdminVideoComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/levels',
        component: LevelComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/courses',
        component: CourseComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/units',
        component: UnitComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/unitDetail',
        component: UnitDetailComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/tests',
        component: QuizComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'admin/placements',
        component: PlacementComponent,
        canActivate: [authGuard, roleGuard],
        data: adminModerator,
      },
      {
        path: 'user/units',
        component: MyProgressComponent,
        canActivate: [authGuard],
      },
      { path: 'user/search', component: SearchComponent },
      { path: 'vocabulary', component: VocabularyHubComponent },
      {
        path: 'ai-tutor',
        component: AiTutorPageComponent,
        canActivate: [authGuard],
      },
      {
        path: 'speaking-lab',
        component: SpeakingLabPageComponent,
        canActivate: [authGuard],
      },
      {
        path: 'guild',
        component: GamificationArenaPageComponent,
        canActivate: [authGuard],
      },
      {
        path: 'user/quiz',
        component: QuizLearnComponent,
        canActivate: [authGuard],
      },
      {
        path: 'user/tests',
        component: PlacementUserComponent,
        canActivate: [authGuard],
      },
    ],
  },
];
