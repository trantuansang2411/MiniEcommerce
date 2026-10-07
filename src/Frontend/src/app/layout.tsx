import type { Metadata } from "next";
import "./globals.css";
import { AppProvider } from "@/shared/providers/app-provider";
import { SiteChrome } from "@/shared/components/site-chrome";

export const metadata: Metadata = {
  title: "MiniStore",
  description: "MiniStore ecommerce frontend",
  icons: {
    icon: "/brand/AnhEcommerc.png",
    apple: "/brand/AnhEcommerc.png",
  },
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi">
      <body>
        <AppProvider>
          <SiteChrome>{children}</SiteChrome>
        </AppProvider>
      </body>
    </html>
  );
}
