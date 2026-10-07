import * as React from "react";
import { cn } from "@/shared/utils/cn";

function Card({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("rounded-xl border border-slate-200/80 bg-white text-slate-950 shadow-[0_1px_2px_rgba(15,32,55,.04),0_12px_30px_rgba(15,32,55,.05)]", className)} {...props} />;
}

function CardContent({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("p-4", className)} {...props} />;
}

export { Card, CardContent };
