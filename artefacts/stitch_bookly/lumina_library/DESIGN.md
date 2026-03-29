# Design System Specification: The Digital Atelier

## 1. Overview & Creative North Star
**Creative North Star: The Curated Library**
This design system rejects the "SaaS-in-a-box" aesthetic in favor of a high-end, editorial experience. We treat book management not as a database task, but as a curatorial art form. The interface mimics the experience of a modern, sun-drenched gallery: expansive whitespace, light-refracting surfaces (glassmorphism), and a sophisticated typographic hierarchy.

To break the "template" look, we utilize **Intentional Asymmetry**. Hero sections should feature overlapping elements—such as a book cover bleeding off a floating glass card—to create a sense of depth and physical presence. We move away from rigid, boxed grids and toward a fluid, layered canvas.

---

## 2. Colors & Tonal Architecture
The palette is rooted in sophisticated neutrals to let book cover art provide the primary color, while using a vibrant indigo (`primary: #4a40e0`) to denote tech-forward intelligence.

### The "No-Line" Rule
**Explicit Instruction:** Designers are prohibited from using 1px solid borders for sectioning. Structural definition must be achieved through background shifts (e.g., a `surface-container-low` sidebar against a `surface` main content area). 

### Surface Hierarchy & Nesting
Treat the UI as a physical stack of semi-transparent materials.
- **Nesting Logic:** Use `surface-container-lowest` (#ffffff) for the most prominent interactive cards sitting atop a `surface-container-low` (#eef1f3) background.
- **The Glass & Gradient Rule:** For primary actions and high-level navigation, use a subtle linear gradient: `primary` (#4a40e0) to `primary-container` (#9795ff) at a 135° angle. This adds "soul" to the interface that flat fills cannot replicate.
- **Glassmorphism:** For floating overlays (modals, popovers), use `surface` with 70% opacity and a `20px` to `40px` backdrop-blur.

---

## 3. Typography
We pair two modern sans-serifs to distinguish between "Reading" and "Operating."

*   **Display & Headlines (Manrope):** Used for titles and headers. Manrope’s geometric yet warm nature feels authoritative yet approachable. 
    *   *Scale:* Use `display-lg` (3.5rem) for hero moments to create high-contrast editorial impact.
*   **Body & Labels (Inter):** Used for all functional data. Inter provides maximum readability at small scales for book metadata (ISBNs, page counts).

**Editorial Hierarchy:** Always maintain a significant jump between `headline-md` and `body-lg`. If everything is important, nothing is. Use `tertiary` (#933880) sparingly for "Editorial Notes" or "Curator Picks" to provide a sophisticated counter-point to the indigo primary.

---

## 4. Elevation & Depth
In this system, depth is a functional tool, not a decoration.

*   **The Layering Principle:** Avoid shadows where tonal shifts can work. A `surface-container-highest` (#d9dde0) element on a `surface` (#f5f7f9) background creates a "recessed" feel without visual clutter.
*   **Ambient Shadows:** For floating elements (like a "Current Read" card), use extra-diffused shadows.
    *   *Spec:* `0px 20px 40px rgba(44, 47, 49, 0.06)`. Note the use of `on-surface` (#2c2f31) as the shadow base rather than pure black to keep the light "natural."
*   **The "Ghost Border" Fallback:** If a container requires definition against an identical background color, use a `1px` stroke of `outline-variant` (#abadaf) at **15% opacity**. 100% opaque borders are strictly forbidden.

---

## 5. Components

### Cards & Lists
*   **The Zero-Divider Policy:** Forbid horizontal lines between list items. Use the **Spacing Scale** `spacing-6` (2rem) to create separation through "breathing room" or alternate background tones.
*   **Book Cards:** Use `roundedness-lg` (2rem). The cover art should have a `roundedness-sm` (0.5rem) and sit within a `surface-container-lowest` floating card.

### Buttons
*   **Primary:** Gradient fill (`primary` to `primary-container`), `roundedness-full`, and a soft ambient shadow.
*   **Secondary:** Ghost style. No background, `primary` text, and the "Ghost Border" (15% opacity `primary`) only on hover.

### Input Fields
*   **Text Inputs:** Soft `surface-container-highest` fills with `roundedness-md`. On focus, the background transitions to `surface-container-lowest` with a `2px` `primary` "Ghost Border."

### Specialized Components
*   **Glass Reader Overlays:** For reading progress or annotations, use a `backdrop-blur` container that allows the book's typography to subtly bleed through the UI.
*   **The "Shelf" Progress Bar:** A thick (`spacing-2`) bar using `surface-container-highest` as the track and a `primary-to-tertiary` gradient for the progress fill.

---

## 6. Do’s and Don’ts

### Do
*   **Do** use `spacing-16` and `spacing-20` for page margins to emphasize the "Premium" feel.
*   **Do** overlap images over container edges to break the "boxy" SaaS feel.
*   **Do** use `tertiary-container` (#ff95e3) for subtle highlights in "Wishlist" or "Favorite" states.

### Don’t
*   **Don’t** use pure black (#000000) for text. Use `on-surface` (#2c2f31) to maintain a soft, ink-on-paper quality.
*   **Don’t** use standard `1rem` border radiuses. We are committed to `lg` (2rem) and `xl` (3rem) to maintain a friendly, modern silhouette.
*   **Don’t** use sharp, high-contrast shadows. If the shadow is easily visible, it’s too dark. It should be felt, not seen.