export type ShellNavIcon =
  | 'spark'
  | 'path'
  | 'book'
  | 'search'
  | 'target'
  | 'users'
  | 'video'
  | 'stack'
  | 'clipboard'
  | 'user';

export interface ShellNavItem {
  label: string;
  path: string;
  icon: ShellNavIcon;
  /** If set, item is visible only when role is in the list. If omitted, visible to everyone. */
  roles?: string[];
}
