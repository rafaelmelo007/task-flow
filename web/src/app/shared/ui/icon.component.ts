import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Monochrome line-icon system (Lucide / Feather style).
 * Every icon is a single-stroke SVG that inherits `currentColor` and a
 * consistent stroke width, so icons stay visually uniform everywhere.
 *
 * Usage: <app-icon name="edit" [size]="18" />
 */
export type IconName =
  | 'check'
  | 'logo'
  | 'edit'
  | 'trash'
  | 'calendar'
  | 'search'
  | 'plus'
  | 'inbox'
  | 'circle'
  | 'circle-dot'
  | 'circle-dashed'
  | 'check-circle'
  | 'chevrons-up'
  | 'chevron-down'
  | 'equal'
  | 'flag'
  | 'log-out'
  | 'filter'
  | 'chevron-left'
  | 'chevron-right'
  | 'alert'
  | 'x'
  | 'mail'
  | 'lock';

@Component({
  selector: 'app-icon',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      [attr.stroke-width]="strokeWidth()"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      @switch (name()) {
        @case ('logo') { <path d="M20 6 9 17l-5-5" /> }
        @case ('check') { <path d="M20 6 9 17l-5-5" /> }
        @case ('edit') {
          <path d="M12 20h9" />
          <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
        }
        @case ('trash') {
          <path d="M3 6h18" />
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
          <line x1="10" y1="11" x2="10" y2="17" />
          <line x1="14" y1="11" x2="14" y2="17" />
        }
        @case ('calendar') {
          <rect x="3" y="4" width="18" height="18" rx="2" />
          <line x1="16" y1="2" x2="16" y2="6" />
          <line x1="8" y1="2" x2="8" y2="6" />
          <line x1="3" y1="10" x2="21" y2="10" />
        }
        @case ('search') {
          <circle cx="11" cy="11" r="8" />
          <path d="m21 21-4.3-4.3" />
        }
        @case ('plus') {
          <line x1="12" y1="5" x2="12" y2="19" />
          <line x1="5" y1="12" x2="19" y2="12" />
        }
        @case ('inbox') {
          <path d="M22 12h-6l-2 3h-4l-2-3H2" />
          <path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z" />
        }
        @case ('circle') { <circle cx="12" cy="12" r="9" /> }
        @case ('circle-dot') {
          <circle cx="12" cy="12" r="9" />
          <circle cx="12" cy="12" r="3.5" fill="currentColor" stroke="none" />
        }
        @case ('circle-dashed') {
          <path d="M10.1 2.18a9.93 9.93 0 0 1 3.8 0" />
          <path d="M17.6 3.71a9.95 9.95 0 0 1 2.69 2.7" />
          <path d="M21.82 10.1a9.93 9.93 0 0 1 0 3.8" />
          <path d="M20.29 17.6a9.95 9.95 0 0 1-2.7 2.69" />
          <path d="M13.9 21.82a9.94 9.94 0 0 1-3.8 0" />
          <path d="M6.4 20.29a9.95 9.95 0 0 1-2.69-2.7" />
          <path d="M2.18 13.9a9.93 9.93 0 0 1 0-3.8" />
          <path d="M3.71 6.4a9.95 9.95 0 0 1 2.7-2.69" />
        }
        @case ('check-circle') {
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <path d="m9 11 3 3L22 4" />
        }
        @case ('chevrons-up') {
          <path d="m17 11-5-5-5 5" />
          <path d="m17 18-5-5-5 5" />
        }
        @case ('chevron-down') { <path d="m6 9 6 6 6-6" /> }
        @case ('equal') {
          <line x1="5" y1="9" x2="19" y2="9" />
          <line x1="5" y1="15" x2="19" y2="15" />
        }
        @case ('flag') {
          <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z" />
          <line x1="4" y1="22" x2="4" y2="15" />
        }
        @case ('log-out') {
          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
          <polyline points="16 17 21 12 16 7" />
          <line x1="21" y1="12" x2="9" y2="12" />
        }
        @case ('filter') { <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" /> }
        @case ('chevron-left') { <polyline points="15 18 9 12 15 6" /> }
        @case ('chevron-right') { <polyline points="9 18 15 12 9 6" /> }
        @case ('alert') {
          <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
          <line x1="12" y1="9" x2="12" y2="13" />
          <line x1="12" y1="17" x2="12.01" y2="17" />
        }
        @case ('x') {
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        }
        @case ('mail') {
          <rect x="2" y="4" width="20" height="16" rx="2" />
          <path d="m22 7-10 5L2 7" />
        }
        @case ('lock') {
          <rect x="3" y="11" width="18" height="11" rx="2" />
          <path d="M7 11V7a5 5 0 0 1 10 0v4" />
        }
      }
    </svg>
  `,
  styles: [
    `:host { display: inline-flex; align-items: center; justify-content: center; line-height: 0; }`,
  ],
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input(18);
  readonly strokeWidth = input(1.75);
}
